﻿using System;
using Preparation.Interface;
using Preparation.Utility;

namespace Preparation.GameObj
{
    public abstract partial class Character : GameObj, ICharacter	// 负责人LHR摆烂中...该文件下抽象部分类已基本完工，剩下的在buffmanager里写
    {
        public readonly object propLock = new object();
        private object beAttackedLock = new object();
        public object PropLock => propLock;

        #region 角色的基本属性及方法，包括与道具、子弹的交互方法
        /// <summary>
        /// 装弹冷却/近战攻击冷却
        /// </summary>
        protected int cd;
        public int CD
        {
            get => cd;
            private set
            {
                lock (gameObjLock)
                {
                    cd = value;
                    //Debugger.Output(this, string.Format("'s CD has been set to: {0}.", value));
                }
            }
        }
        public int OrgCD { get; protected set; }	// 原初冷却
        protected int maxBulletNum;
        public int MaxBulletNum => maxBulletNum;	// 人物最大子弹数
        protected int bulletNum;	
        public int BulletNum => bulletNum;  // 目前持有的子弹数
        public int MaxHp { get; protected set; }    // 最大血量
        protected int hp;
        public int HP => hp;    // 当前血量
        private int deathCount = 0;       
        public int DeathCount => deathCount;  // 玩家的死亡次数
        protected int ap;   // 当前攻击力
        public int AP
        {
            get => ap;
            private set
            {
                lock (gameObjLock)
                {
                    ap = value;
                    //Debugger.Output(this, "'s AP has been set to: " + value.ToString());
                }
            }
        }
        public int OrgAp { get; protected set; }    // 原初攻击力
        private int score;
        public int Score => score;  // 当前分数

        private Prop? propInventory;
        public Prop? PropInventory  //持有的道具
        {
            get => propInventory;
            set
            {
                lock (gameObjLock)
                {
                    propInventory = value;
                    //Debugger.Output(this, " picked the prop: " + (holdProp == null ? "null" : holdProp.ToString()));
                }
            }
        }
        /// <summary>
        /// 使用物品栏中的道具
        /// </summary>
        /// <returns>被使用的道具</returns>
        public Prop? UseProp()
        {
            lock (gameObjLock)
            {
                var oldProp = PropInventory;
                PropInventory = null;
                return oldProp;
            }
        }
        /// <summary>
        /// 是否正在更换道具（包括捡起与抛出）
        /// </summary>
        private bool isModifyingProp = false;
        public bool IsModifyingProp
        {
            get => isModifyingProp;
            set
            {
                lock (gameObjLock)
                {
                    isModifyingProp = value;
                }
            }
        }

        public abstract BulletType Bullet { get; } //人物的发射子弹类型，射程伤害等信息存在具体子弹里
        /// <summary>
        /// 进行一次远程攻击
        /// </summary>
        /// <param name="posOffset"></param>
        /// <param name="bulletRadius"></param>
        /// <param name="basicBulletMoveSpeed"></param>
        /// <returns>攻击操作发出的子弹</returns>
        public Bullet? RemoteAttack(XYPosition posOffset, int bulletRadius, int basicBulletMoveSpeed)
        {
            if (TrySubBulletNum()) return ProduceOneBullet(posOffset, bulletRadius, basicBulletMoveSpeed);
            else return null;
        }
        /// <summary>
        /// 产生一颗子弹
        /// </summary>
        /// <param name="posOffset"></param>
        /// <param name="bulletRadius"></param>
        /// <param name="basicBulletMoveSpeed"></param>
        /// <returns>产生的子弹</returns>
        protected abstract Bullet ProduceOneBullet(XYPosition posOffset, int bulletRadius, int basicBulletMoveSpeed);

        /// <summary>
        /// 尝试将子弹数量减1
        /// </summary>
        /// <returns>减操作是否成功</returns>
        private bool TrySubBulletNum()	
        {
            lock (gameObjLock)
            {
                if (bulletNum > 0)
                {
                    --bulletNum;
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// 尝试将子弹数量加1
        /// </summary>
        /// <returns>加操作是否成功</returns>
        public bool TryAddBulletNum()
        {
            lock (gameObjLock)
            {
                if (bulletNum < maxBulletNum)
                {
                    ++bulletNum;
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// 尝试加血
        /// </summary>
        /// <param name="add">欲加量</param>
        /// <returns>加操作是否成功</returns>
        public bool TryAddHp(int add)
        {
            lock (gameObjLock)
            {
                if(hp < MaxHp)
                {
                    hp = MaxHp > hp + add ? hp + add : MaxHp;
                    //Debugger.Output(this, " hp has added to: " + hp.ToString());
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// 尝试减血
        /// </summary>
        /// <param name="sub">减血量</param>
        /// <returns>减操作是否成功</returns>
        public bool TrySubHp(int sub)
        {
            lock (gameObjLock)
            {
                if (hp > 0)
                {
                    hp = 0 >= hp - sub ? 0 : hp - sub;
                    //Debugger.Output(this, " hp has subed to: " + hp.ToString());
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// 增加死亡次数
        /// </summary>
        /// <returns>当前死亡次数</returns>
        private int AddDeathCount()
        {
            lock (gameObjLock)
            {
                ++deathCount;
                return deathCount;
            }
        }
        /// <summary>
        /// 加分
        /// </summary>
        /// <param name="add">增加量</param>
        public void AddScore(int add)
        {
            lock (gameObjLock)
            {
                score += add;
                //Debugger.Output(this, " 's score has been added to: " + score.ToString());
            }
        }
        /// <summary>
        /// 减分
        /// </summary>
        /// <param name="sub">减少量</param>
        public void SubScore(int sub)
        {
            lock (gameObjLock)
            {
                score -= sub;
                //Debugger.Output(this, " 's score has been subed to: " + score.ToString());
            }
        }
        /// <summary>
        /// 遭受攻击
        /// </summary>
        /// <param name="subHP"></param>
        /// <param name="hasSpear"></param>
        /// <param name="attacker">伤害来源</param>
        /// <returns>是否因该攻击而死</returns>
        public bool BeAttack(int subHP, bool hasSpear, Character? attacker)
        {
            lock (beAttackedLock)
            {
                if (hp <= 0) return false;
                if (!(attacker?.TeamID == this.TeamID))
                {
                    if (hasSpear || !HasShield) TrySubHp(subHP);
                    if (hp <= 0) TryActivatingTotem();
                    if (Job == JobType.Job6) attacker?.BeBounced(subHP * 3 / 4, this.HasSpear, this);   //职业6可以反弹伤害
                }
                else if (attacker?.Job == JobType.Job6)
                {
                    TryAddHp(subHP * 6);               // 职业六回血6倍
                }
                return hp <= 0;
            }
        }
        /// <summary>
        /// 攻击被反弹，反弹伤害不会再被反弹
        /// </summary>
        /// <param name="subHP"></param>
        /// <param name="hasSpear"></param>
        /// <param name="bouncer">反弹伤害者</param>
        /// <returns>是否因反弹伤害而死</returns>
        private bool BeBounced(int subHP, bool hasSpear, Character? bouncer)
        {
            lock (beAttackedLock)
            {
                if (hp <= 0) return false;
                if (!(bouncer?.TeamID == this.TeamID))
                {
                    if (hasSpear || !HasShield) TrySubHp(subHP);
                    if (hp <= 0) TryActivatingTotem();
                }
                else if (attacker?.Job == JobType.Job6)
                {
                    TryAddHp(subHP * 6);               // 职业六回血6倍
                }
                return hp <= 0;
            }
        }
        /// <summary>
        /// 角色所属队伍ID
        /// </summary>
        private long teamID = long.MaxValue;
        public long TeamID
        {
            get => teamID;
            set
            {
                lock (gameObjLock)
                {
                    teamID = value;
                    //Debugger.Output(this, " joins in the team: " + value.ToString());
                }
            }
        }
        /// <summary>
        /// 角色携带的信息
        /// </summary>
        private string message = "THUAI5";
        public string Message
        {
            get => message;
            set
            {
                lock (gameObjLock)
                {
                    message = value;
                }
            }
        }
        #endregion

        #region 角色拥有的buff相关属性、方法（目前还是完全照搬的）
        public void AddMoveSpeed(double add, int buffTime) => buffManeger.AddMoveSpeed(add, buffTime, newVal => { MoveSpeed = newVal; }, OrgMoveSpeed);

        public void AddAP(double add, int buffTime) => buffManeger.AddAP(add, buffTime, newVal => { AP = newVal; }, OrgAp);

        public void ChangeCD(double discount, int buffTime) => buffManeger.ChangeCD(discount, buffTime, newVal => { CD = newVal; }, OrgCD);

        public void AddShield(int shieldTime) => buffManeger.AddShield(shieldTime);
        public bool HasShield => buffManeger.HasShield;

        public void AddLIFE(int LIFETime) => buffManeger.AddLIFE(LIFETime);
        public bool HasTotem => buffManeger.HasTotem;

        public void AddSpear(int spearTime) => buffManeger.AddSpear(spearTime);
        public bool HasSpear => buffManeger.HasSpear;

        private void TryActivatingTotem()
        {
            if (buffManeger.TryActivatingTotem())
            {
                hp = MaxHp;
            }
        }
        #endregion
        public override void Reset()
        {
            AddDeathCount();
            base.Reset();
            this.moveSpeed = OrgMoveSpeed;
            hp = MaxHp;
            ap = OrgAp;
            PropInventory = null;
            bulletNum = maxBulletNum / 2;
            buffManeger.ClearAll();
        }
        public override bool IsRigid => true;
        protected override bool IgnoreCollideExecutor(IGameObj targetObj)
        {
            if (targetObj is BirthPoint && object.ReferenceEquals(((BirthPoint)targetObj).Parent, this))    // 自己的出生点可以忽略碰撞
            {
                return true;
            }
            else if (targetObj is Mine && ((Mine)targetObj).Parent?.TeamID == TeamID)   // 自己队的炸弹忽略碰撞
            {
                return true;
            }
            return false;
        }
        public Character(XYPosition initPos, int initRadius, PlaceType initPlace, int initSpeed) :base(initPos,initRadius,initPlace)
        {
            this.CanMove = true;
            this.Type = GameObjType.Character;
            this.moveSpeed = initSpeed;
        }
    }
}
