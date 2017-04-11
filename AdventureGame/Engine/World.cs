using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public static class World
    {
        public static readonly List<Item> Items = new List<Item>();
        public static readonly List<Enemy> Enemies = new List<Enemy>();
        public static readonly List<Quest> Quests = new List<Quest>();
        public static readonly List<Location> Locations = new List<Location>();

        public const int ITEM_ID_BROKEN_SWORD = 1;       //weapon
        public const int ITEM_ID_BANDIT_HEAD = 2;
        public const int ITEM_ID_BLACK_PEARL = 3;
        public const int ITEM_ID_SERPENT_FANG = 4;
        public const int ITEM_ID_SERPENTSKIN = 5;
        public const int ITEM_ID_BO_STAFF = 6;           //weapon
        public const int ITEM_ID_HEALING_POTION = 7;
        public const int ITEM_ID_SPIDER_FANG = 8;
        public const int ITEM_ID_SPIDER_SILK = 9;
        public const int ITEM_ID_ADVENTURER_KEY = 10;

        public const int ENEMY_ID_BANDIT = 1;
        public const int ENEMY_ID_SERPENT = 2;
        public const int ENEMY_ID_GIANT_SPIDER = 3;

        public const int QUEST_ID_HELP_LIBRARIAN = 1;
        public const int QUEST_ID_CLEAR_FARMERS_FIELD = 2;

        public const int LOCATION_ID_HOME = 1;
        public const int LOCATION_ID_TOWN_SQUARE = 2;
        public const int LOCATION_ID_GUARD_POST = 3;
        public const int LOCATION_ID_LIBRARY = 4;
        public const int LOCATION_ID_LIBRARIAN_ZEN_GARDEN = 5;
        public const int LOCATION_ID_FARMHOUSE = 6;
        public const int LOCATION_ID_FARM_FIELD = 7;
        public const int LOCATION_ID_BRIDGE = 8;
        public const int LOCATION_ID_SPIDER_FIELD = 9;

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
            Items.Add(new Item(ITEM_ID_SPIDER_FANG, "Spider fang", "Spider fangs"));
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
            giantSpider.LootTable.Add(new LootItem(ItemByID(ITEM_ID_SPIDER_FANG), 75, true));
            giantSpider.LootTable.Add(new LootItem(ItemByID(ITEM_ID_SPIDER_SILK), 25, false));

            Enemies.Add(bandit);
            Enemies.Add(serpent);
            Enemies.Add(giantSpider);
        }

        private static void PopulateQuests()
        {
            Quest helpLibrarian =
                new Quest(
                    QUEST_ID_HELP_LIBRARIAN,
                    "Kill the bandits in the librarian's Zen garden (some of them can have a stolen Black pearl)",
                    "Kill the bandits in the library and bring back 3 Bandits' heads to the librarian for proof that you're not one of them. You will receive a healing potion and 10 gold.", 40, 20);

            helpLibrarian.QuestCompletionItems.Add(new QuestCompletionItem(ItemByID(ITEM_ID_BANDIT_HEAD), 3));

            helpLibrarian.RewardItem = ItemByID(ITEM_ID_HEALING_POTION);

            Quest clearFarmersField =
                new Quest(
                    QUEST_ID_CLEAR_FARMERS_FIELD,
                    "Clear the farmer's field",
                    "Kill the serpents in the farmer's field and bring back 3 serpent fangs. You will receive an adventurer's pass and 20 gold pieces.", 30, 10);

            clearFarmersField.QuestCompletionItems.Add(new QuestCompletionItem(ItemByID(ITEM_ID_SERPENT_FANG), 3));

            clearFarmersField.RewardItem = ItemByID(ITEM_ID_ADVENTURER_KEY);

            Quests.Add(helpLibrarian);
            Quests.Add(clearFarmersField);
        }

        private static void PopulateLocations()
        {
            // Create each location
            Location home = new Location(LOCATION_ID_HOME, "Home", "Your house.", @"..\\..\\..\\Engine\\src\\game map\\Home.png");

            Location townSquare = new Location(LOCATION_ID_TOWN_SQUARE, "Town square", "You see rats eating bird shits in the drained fountain.", @"..\\..\\..\\Engine\\src\\game map\\TownSquare.png");

            Location library = new Location(LOCATION_ID_LIBRARY, "Library", "*achoo* There are millions of books here!", @"..\\..\\..\\Engine\\src\\game map\\Library.png");
            library.QuestAvailableHere = QuestByID(QUEST_ID_HELP_LIBRARIAN);

            Location zenGarden = new Location(LOCATION_ID_LIBRARIAN_ZEN_GARDEN, "Librarian's Zen garden", "Wow! Many plants are growing here. The fountain tho... Then you see them. Bandits!", @"..\\..\\..\\Engine\\src\\game map\\ZenGarden.png");
            zenGarden.EnemyLivingHere = EnemyByID(ENEMY_ID_BANDIT);

            Location farmhouse = new Location(LOCATION_ID_FARMHOUSE, "Farmhouse", "You see an old farmer in front of you.", @"..\\..\\..\\Engine\\src\\game map\\Farm.png");
            farmhouse.QuestAvailableHere = QuestByID(QUEST_ID_CLEAR_FARMERS_FIELD);

            Location farmersField = new Location(LOCATION_ID_FARM_FIELD, "Farmer's field", "You see rows of veggies growing here.", @"..\\..\\..\\Engine\\src\\game map\\Fields.png");
            farmersField.EnemyLivingHere = EnemyByID(ENEMY_ID_SERPENT);

            Location guardPost = new Location(LOCATION_ID_GUARD_POST, "Guard post", "There is a large dude here. He asks your to show your key.", @"..\\..\\..\\Engine\\src\\game map\\GuardPost.png", ItemByID(ITEM_ID_ADVENTURER_KEY));

            Location bridge = new Location(LOCATION_ID_BRIDGE, "Bridge", "A stone bridge crosses a wide river.", @"..\\..\\..\\Engine\\src\\game map\\Bridge.png");

            Location spiderWoods = new Location(LOCATION_ID_SPIDER_FIELD, "Forest", "You see spider webs all over the place, covering the trees, the grass, everywhere in the forest.", @"..\\..\\..\\Engine\\src\\game map\\Forest.png");
            spiderWoods.EnemyLivingHere = EnemyByID(ENEMY_ID_GIANT_SPIDER);

            // Link the locations together
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

            // Add the locations to the static list
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
            {
                if (item.ID == id)
                {
                    return item;
                }
            }

            return null;
        }

        public static Enemy EnemyByID(int id)
        {
            foreach (Enemy enemy in Enemies)
            {
                if (enemy.ID == id)
                {
                    return enemy;
                }
            }

            return null;
        }

        public static Quest QuestByID(int id)
        {
            foreach (Quest quest in Quests)
            {
                if (quest.ID == id)
                {
                    return quest;
                }
            }

            return null;
        }

        public static Location LocationByID(int id)
        {
            foreach (Location location in Locations)
            {
                if (location.ID == id)
                {
                    return location;
                }
            }

            return null;
        }
    }
}
