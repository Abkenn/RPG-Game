using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class LivingCreature
    {
        public int CurrentHitPoints { get; set; }
        public int MaximumHitPoints { get; set; }

        public LivingCreature(int currentHP, int maximumHP)
        {
            CurrentHitPoints = currentHP;
            MaximumHitPoints = maximumHP;
        }
    }
}
