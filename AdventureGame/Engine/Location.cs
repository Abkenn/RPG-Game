﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Engine
{
    public class Location
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Item ItemRequiredToEnter { get; set; }
        public Quest QuestAvailableHere { get; set; }
        public Enemy EnemyLivingHere { get; set; }
        public Location LocationToNorth { get; set; }
        public Location LocationToEast { get; set; }
        public Location LocationToSouth { get; set; }
        public Location LocationToWest { get; set; }
        public Image Minimap { get; set; }


        public Location(int id, string name, string description, Image minimap, Item itemReqToEnter = null, Quest questAvailableHere = null, Enemy enemyLivingHere = null)
        {
            ID = id;
            Name = name;
            Description = description;
            ItemRequiredToEnter = itemReqToEnter;
            QuestAvailableHere = questAvailableHere;
            EnemyLivingHere = enemyLivingHere;

            Minimap = minimap;
        }

    }
}
