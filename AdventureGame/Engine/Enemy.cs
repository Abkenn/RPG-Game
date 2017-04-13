using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Enemy : LivingCreature
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int MaximumDamage { get; set; }
        public int RewardExperiencePoints { get; set; }
        public int RewardGold { get; set; }
        public List<LootItem> LootTable { get; set; }


        public Enemy(int id, string name, int maximumDamage, int rewardXP, int rewardGold, int currentHP, int maximumHP) : base(currentHP, maximumHP)
        {
            ID = id;
            Name = name;
            MaximumDamage = maximumDamage;
            RewardExperiencePoints = rewardXP;
            RewardGold = rewardGold;

            LootTable = new List<LootItem>();
        }

    }
}
