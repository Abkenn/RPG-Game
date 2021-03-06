﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public static class World
    {
        public static List<Item> Items { get; } = new List<Item>();
        public static List<Enemy> Enemies { get; } = new List<Enemy>();
        public static List<Quest> Quests { get; } = new List<Quest>();
        public static List<Location> Locations { get; } = new List<Location>();


        // Weapons
        public const int ITEM_ID_BROKEN_SWORD = 1;
        public const int ITEM_ID_BO_STAFF = 2;

        // Potions
        public const int ITEM_ID_HEALING_POTION = 401;

        // Quest items
        public const int ITEM_ID_BANDIT_HEAD = 501;
        public const int ITEM_ID_SERPENT_FANG = 502;
        public const int ITEM_ID_SPIDER_VENOM_SAC = 503;

        // Keys
        public const int ITEM_ID_ADVENTURER_KEY = 801;

        // Other items
        public const int ITEM_ID_BLACK_PEARL = 1001;
        public const int ITEM_ID_SERPENTSKIN = 1002;
        public const int ITEM_ID_SPIDER_SILK = 1003;

        // Enemies
        public const int ENEMY_ID_BANDIT = 3001;
        public const int ENEMY_ID_SERPENT = 3002;
        public const int ENEMY_ID_GIANT_SPIDER = 3003;

        // Quests
        public const int QUEST_ID_HELP_LIBRARIAN = 7001;
        public const int QUEST_ID_CLEAR_FARMERS_FIELD = 7002;

        // Locations
        public const int LOCATION_ID_HOME = 8001;
        public const int LOCATION_ID_TOWN_SQUARE = 8002;
        public const int LOCATION_ID_GUARD_POST = 8003;
        public const int LOCATION_ID_LIBRARY = 8004;
        public const int LOCATION_ID_LIBRARIAN_ZEN_GARDEN = 8005;
        public const int LOCATION_ID_FARMHOUSE = 8006;
        public const int LOCATION_ID_FARM_FIELD = 8007;
        public const int LOCATION_ID_BRIDGE = 8008;
        public const int LOCATION_ID_SPIDER_FIELD = 8009;

        // Other ID's from 10001...

        static World()
        {
            PopulateItems();
            PopulateEnemies();
            PopulateQuests();
            PopulateLocations();
        }

        private static void PopulateItems()
        {
            Items.Add(new Weapon(ITEM_ID_BROKEN_SWORD, "Broken sword", "Broken swords", 0, 5));
            Items.Add(new Item(ITEM_ID_BANDIT_HEAD, "Bandit's head", "Bandits' heads"));
            Items.Add(new Item(ITEM_ID_BLACK_PEARL, "Black pearl", "Black pearls"));
            Items.Add(new Item(ITEM_ID_SERPENT_FANG, "Serpent fang", "Serpent fangs"));
            Items.Add(new Item(ITEM_ID_SERPENTSKIN, "Serpentskin", "Serpentskins"));
            Items.Add(new Weapon(ITEM_ID_BO_STAFF, "Bo", "Bo Staffs", 3, 10));
            Items.Add(new HealingPotion(ITEM_ID_HEALING_POTION, "Healing potion", "Healing potions", 5));
            Items.Add(new Item(ITEM_ID_SPIDER_VENOM_SAC, "Spider fang", "Spider fangs"));
            Items.Add(new Item(ITEM_ID_SPIDER_SILK, "Spider silk", "Spider silks"));
            Items.Add(new Item(ITEM_ID_ADVENTURER_KEY, "Adventurer key", "Adventurer keys"));
        }

        private static void PopulateEnemies()
        {
            Enemy bandit = new Enemy(ENEMY_ID_BANDIT, "Bandit", 6, 4, 10, 6, 6);
            bandit.LootTable.Add(new LootItem(ItemByID(ITEM_ID_BANDIT_HEAD), 100, true));
            bandit.LootTable.Add(new LootItem(ItemByID(ITEM_ID_BLACK_PEARL), 40, false));

            Enemy serpent = new Enemy(ENEMY_ID_SERPENT, "Serpent", 5, 2, 6, 3, 3);
            serpent.LootTable.Add(new LootItem(ItemByID(ITEM_ID_SERPENT_FANG), 75, false));
            serpent.LootTable.Add(new LootItem(ItemByID(ITEM_ID_SERPENTSKIN), 100, true));

            Enemy giantSpider = new Enemy(ENEMY_ID_GIANT_SPIDER, "Giant spider", 10, 5, 40, 10, 10);
            giantSpider.LootTable.Add(new LootItem(ItemByID(ITEM_ID_SPIDER_VENOM_SAC), 75, true));
            giantSpider.LootTable.Add(new LootItem(ItemByID(ITEM_ID_SPIDER_SILK), 25, false));

            Enemies.Add(bandit);
            Enemies.Add(serpent);
            Enemies.Add(giantSpider);
        }

        private static void PopulateQuests()
        {
            Quest helpLibrarian = new Quest(
                    QUEST_ID_HELP_LIBRARIAN,
                    "Kill the bandits in the librarian's Zen garden (some of them can have a stolen Black pearl)",
                    "Kill the bandits in the library and bring back 3 Bandits' heads to the librarian for proof that you're not one of them. You will receive a healing potion and 10 gold.", 
                    40, 20);
            helpLibrarian.QuestCompletionItems.Add(new QuestCompletionItem(ItemByID(ITEM_ID_BANDIT_HEAD), 3));
            helpLibrarian.RewardItem = ItemByID(ITEM_ID_HEALING_POTION);

            Quest clearFarmersField = new Quest(
                    QUEST_ID_CLEAR_FARMERS_FIELD,
                    "Clear the farmer's field",
                    "Kill the serpents in the farmer's field and bring back 3 serpent fangs. You will receive an adventurer's pass and 20 gold pieces.", 
                    30, 10);
            clearFarmersField.QuestCompletionItems.Add(new QuestCompletionItem(ItemByID(ITEM_ID_SERPENT_FANG), 3));
            clearFarmersField.RewardItem = ItemByID(ITEM_ID_ADVENTURER_KEY);

            Quests.Add(helpLibrarian);
            Quests.Add(clearFarmersField);
        }

        private static void PopulateLocations()
        {
            // Създаване на всички локации
            Location home = new Location(LOCATION_ID_HOME, "Home", "Your house.", Properties.Resources.Home);

            Location townSquare = new Location(LOCATION_ID_TOWN_SQUARE, "Town square", "You see rats eating bird shits in the drained fountain.", Properties.Resources.TownSquare);

            Location library = new Location(LOCATION_ID_LIBRARY, "Library", "*achoo* There are millions of books here!", Properties.Resources.Library);
            library.QuestAvailableHere = QuestByID(QUEST_ID_HELP_LIBRARIAN);

            Location zenGarden = new Location(LOCATION_ID_LIBRARIAN_ZEN_GARDEN, "Librarian's Zen garden", "Wow! Many plants are growing here. The fountain tho... Then you see them. Bandits!", Properties.Resources.ZenGarden);
            zenGarden.EnemyLivingHere = EnemyByID(ENEMY_ID_BANDIT);

            Location farmhouse = new Location(LOCATION_ID_FARMHOUSE, "Farmhouse", "You see an old farmer in front of you.", Properties.Resources.Farm);
            farmhouse.QuestAvailableHere = QuestByID(QUEST_ID_CLEAR_FARMERS_FIELD);

            Location farmersField = new Location(LOCATION_ID_FARM_FIELD, "Farmer's field", "You see rows of veggies growing here.", Properties.Resources.Fields);
            farmersField.EnemyLivingHere = EnemyByID(ENEMY_ID_SERPENT);

            Location guardPost = new Location(LOCATION_ID_GUARD_POST, "Guard post", "There is a large dude here. He asks your to show your key.", Properties.Resources.GuardPost, ItemByID(ITEM_ID_ADVENTURER_KEY));

            Location bridge = new Location(LOCATION_ID_BRIDGE, "Bridge", "A stone bridge crosses a wide river.", Properties.Resources.Bridge);

            Location spiderWoods = new Location(LOCATION_ID_SPIDER_FIELD, "Forest", "You see spider webs all over the place, covering the trees, the grass, everywhere in the forest.", Properties.Resources.Forest);
            spiderWoods.EnemyLivingHere = EnemyByID(ENEMY_ID_GIANT_SPIDER);

            // Свързване на локациите една с друга (чрез посоки)
            home.LocationToNorth = townSquare;

            townSquare.LocationToNorth = library;
            townSquare.LocationToSouth = home;
            townSquare.LocationToEast = guardPost;
            townSquare.LocationToWest = farmhouse;

            farmhouse.LocationToEast = townSquare;
            farmhouse.LocationToWest = farmersField;

            farmersField.LocationToEast = farmhouse;

            library.LocationToSouth = townSquare;
            library.LocationToNorth = zenGarden;

            zenGarden.LocationToSouth = library;

            guardPost.LocationToEast = bridge;
            guardPost.LocationToWest = townSquare;

            bridge.LocationToWest = guardPost;
            bridge.LocationToEast = spiderWoods;

            spiderWoods.LocationToWest = bridge;

            // Добавяне на локациите към статичен списък (който да се достъпва с World.Locations отвсякъде)
            Locations.Add(home);
            Locations.Add(townSquare);
            Locations.Add(guardPost);
            Locations.Add(library);
            Locations.Add(zenGarden);
            Locations.Add(farmhouse);
            Locations.Add(farmersField);
            Locations.Add(bridge);
            Locations.Add(spiderWoods);
        }

        public static Item ItemByID(int id)
        {
            foreach (Item item in Items)
                if (item.ID == id)
                    return item;
            return null;
        }

        public static Enemy EnemyByID(int id)
        {
            foreach (Enemy enemy in Enemies)
                if (enemy.ID == id)
                    return enemy;
            return null;
        }

        public static Quest QuestByID(int id)
        {
            foreach (Quest quest in Quests)
                if (quest.ID == id)
                    return quest;
            return null;
        }

        public static Location LocationByID(int id)
        {
            foreach (Location location in Locations)
                if (location.ID == id)
                    return location;
            return null;
        }

    }
}
