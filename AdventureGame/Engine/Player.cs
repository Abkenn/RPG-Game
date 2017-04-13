using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Engine
{
    public class Player : LivingCreature
    {
        private int gold;
        public int Gold
        {
            get { return gold; }
            set
            {
                gold = value;
                OnPropertyChanged("Gold");
            }
        }

        private int experiencePoints;
        public int ExperiencePoints
        {
            get { return experiencePoints; }
            private set
            {
                experiencePoints = value;
                OnPropertyChanged("ExperiencePoints");
                OnPropertyChanged("Level"); // тук subsrcribe-ваме и Level property-то към събитието, защото нямаме set на Level, единствено го взимаме през get като калкулираме get return-a чрез ExperiencePoints, a и е по-логично при смяна на XP да ъпдейтваме и Level-a, за да не се пропусне евентуално при промяна в level-a спрямо XP, да се ъпдейт и level-a, 2 notifications за 2 различни properties не е добре да се изпращат през 1 property, но проекта е малък и може да се следи за евентуални бъгове, породени от този факт
                OnPropertyChanged("XPNeeded");
            }
        }

        public int XPNeeded { get { return 10 * Level * Level + 90 * Level - ExperiencePoints; } }
        public int Level { get { return (int) Math.Floor(((-90 + Math.Sqrt(8100 + 40 * ExperiencePoints)) / 20 + 1)); } } // xp = 10level^2 + 90level е формулата, ама +1 level, за да не почва от 0
        public BindingList<InventoryItem> Inventory { get; set; }
        public BindingList<PlayerQuest> Quests { get; set; }
        public Location CurrentLocation { get; set; }
        public Weapon CurrentWeapon { get; set; }


        private Player(int currentHP, int maximumHP, int gold, int xp) : base(currentHP, maximumHP)
        {
            Gold = gold;
            ExperiencePoints = xp;

            Inventory = new BindingList<InventoryItem>();
            Quests = new BindingList<PlayerQuest>();
        }


        public static Player CreateDefaultPlayer()
        {
            Player player = new Player(10, 10, 20, 0);
            player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_BROKEN_SWORD), 1));
            player.CurrentLocation = World.LocationByID(World.LOCATION_ID_HOME);

            return player;
        }

        public void AddExperiencePoints(int experiencePointsToAdd)
        {
            ExperiencePoints += experiencePointsToAdd;
            MaximumHitPoints = 2 * (Level * Level + 2 * Level - 1);
            switch(Level)
            {
                case 1: MaximumHitPoints = 10;
                    break;
                case 2: MaximumHitPoints = 21;
                    break;
                case 3: MaximumHitPoints = 32;
                    break;
            }
        }

        public bool HasRequiredItemToEnterThisLocation(Location location)
        {
            if (location.ItemRequiredToEnter == null)
            {
                // Няма required items за тази локация, така че излез с true
                return true;
            }
            // Намерен е предмет от Inventory (true) или 1.1.1) Играчът няма нужният предмет за преминаване (false), при false е нужно съобщение
            return Inventory.Any(ii => ii.Details.ID == location.ItemRequiredToEnter.ID);
        }

        public bool HasThisQuest(Quest quest)
        {
            foreach (PlayerQuest playerQuest in Quests)
                if (playerQuest.Details.ID == quest.ID)
                    return true;
            return false;
        }

        public bool CompletedThisQuest(Quest quest)
        {
            foreach (PlayerQuest playerQuest in Quests)
                if (playerQuest.Details.ID == quest.ID)
                    return playerQuest.IsCompleted;
            return false;
        }

        public bool HasAllQuestCompletionItems(Quest quest)
        {
            // Обхожда цялото Inventory, за да провери дали играчът притежава quest item и дали има нужната бройка да завърши quest-a
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
                if (!Inventory.Any(ii => ii.Details.ID == qci.Details.ID && ii.Quantity >= qci.Quantity))
                    return false;

            // Щом все още не сме излезли от функцията, значи играчът има нужният item и достатъчна бройка от него, за да приключи Quest-a
            return true;
        }

        public void RemoveQuestCompletionItems(Quest quest)
        {
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == qci.Details.ID);
                // Извади нужния брой quest items от броя им в Inventory 
                if (item != null)
                    item.Quantity -= qci.Quantity;
            }
        }

        public void AddItemToInventory(Item itemToAdd)
        {
            // Обхожда Inventory, за да провери дали reward item-a го няма вече в Inventory
            InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToAdd.ID);

            if (item == null) // RewardItem не се съдържа в Inventory щом все още сме във функцията, така че добави reward item-a като нов InventoryItem (в нов слот) с брой 1
                Inventory.Add(new InventoryItem(itemToAdd, 1));
            else // RewardItem се съдържа в Inventory, затова само увеличи броя с 1
                item.Quantity++;
        }

        public void MarkQuestCompleted(Quest quest)
        {
            // Обходи списъка с quests и намери quest-ът, който току що завърши
            PlayerQuest playerQuest = Quests.SingleOrDefault(pq => pq.Details.ID == quest.ID);
            if (playerQuest != null)
                playerQuest.IsCompleted = true; // Отбележи го като completed
        }


        public static Player CreatePlayerFromXmlString(string xmlPlayerData)
        {
            try
            {
                XmlDocument playerData = new XmlDocument();

                playerData.LoadXml(xmlPlayerData);

                int currentHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentHitPoints").InnerText);
                int maximumHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/MaximumHitPoints").InnerText);
                int gold = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/Gold").InnerText);
                int experiencePoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/ExperiencePoints").InnerText);

                Player player = new Player(currentHitPoints, maximumHitPoints, gold, experiencePoints);

                int currentLocationID = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentLocation").InnerText);
                player.CurrentLocation = World.LocationByID(currentLocationID);

                if (playerData.SelectSingleNode("/Player/Stats/CurrentWeapon") != null)
                {
                    int currentWeaponID = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentWeapon").InnerText);
                    player.CurrentWeapon = (Weapon)World.ItemByID(currentWeaponID);
                }

                foreach (XmlNode node in playerData.SelectNodes("/Player/Inventory/InventoryItem"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    int quantity = Convert.ToInt32(node.Attributes["Quantity"].Value);

                    for (int i = 0; i < quantity; i++)
                    {
                        player.AddItemToInventory(World.ItemByID(id));
                    }
                }

                foreach (XmlNode node in playerData.SelectNodes("/Player/PlayerQuests/PlayerQuest"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    bool isCompleted = Convert.ToBoolean(node.Attributes["IsCompleted"].Value);

                    PlayerQuest playerQuest = new PlayerQuest(World.QuestByID(id));
                    playerQuest.IsCompleted = isCompleted;

                    player.Quests.Add(playerQuest);
                }

                return player;
            }
            catch
            {
                // При проблем с Load File - да зареди по подразбиране
                return Player.CreateDefaultPlayer();
            }
        }

        public string ToXmlString()
        {
            XmlDocument playerData = new XmlDocument();

            // Top Node 
            XmlNode player = playerData.CreateElement("Player");
            playerData.AppendChild(player);

            // Stats Node (наследник на player)
            XmlNode stats = playerData.CreateElement("Stats");
            player.AppendChild(stats);

            // Stats' Child Nodes (наследници на Stats Node)
            XmlNode currentHitPoints = playerData.CreateElement("CurrentHitPoints");
            currentHitPoints.AppendChild(playerData.CreateTextNode(this.CurrentHitPoints.ToString()));
            stats.AppendChild(currentHitPoints);

            XmlNode maximumHitPoints = playerData.CreateElement("MaximumHitPoints");
            maximumHitPoints.AppendChild(playerData.CreateTextNode(this.MaximumHitPoints.ToString()));
            stats.AppendChild(maximumHitPoints);

            XmlNode gold = playerData.CreateElement("Gold");
            gold.AppendChild(playerData.CreateTextNode(this.Gold.ToString()));
            stats.AppendChild(gold);

            XmlNode experiencePoints = playerData.CreateElement("ExperiencePoints");
            experiencePoints.AppendChild(playerData.CreateTextNode(this.ExperiencePoints.ToString()));
            stats.AppendChild(experiencePoints);

            XmlNode currentLocation = playerData.CreateElement("CurrentLocation");
            currentLocation.AppendChild(playerData.CreateTextNode(this.CurrentLocation.ID.ToString()));
            stats.AppendChild(currentLocation);

            if (CurrentWeapon != null)
            {
                XmlNode currentWeapon = playerData.CreateElement("CurrentWeapon");
                currentWeapon.AppendChild(playerData.CreateTextNode(this.CurrentWeapon.ID.ToString()));
                stats.AppendChild(currentWeapon);
            }

            // Inventory Node (наследник на player)
            XmlNode inventory = playerData.CreateElement("Inventory");
            player.AppendChild(inventory);

            // InventoryItem Nodes (наследници на inventory node)
            foreach (InventoryItem item in this.Inventory)
            {
                XmlNode inventoryItem = playerData.CreateElement("InventoryItem");

                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = item.Details.ID.ToString();
                inventoryItem.Attributes.Append(idAttribute);

                XmlAttribute quantityAttribute = playerData.CreateAttribute("Quantity");
                quantityAttribute.Value = item.Quantity.ToString();
                inventoryItem.Attributes.Append(quantityAttribute);

                inventory.AppendChild(inventoryItem);
            }

            // PlayerQuests Node (наследник на player)
            XmlNode playerQuests = playerData.CreateElement("PlayerQuests");
            player.AppendChild(playerQuests);

            // PlayerQuest Node (наследници на PlayerQuests)
            foreach (PlayerQuest quest in this.Quests)
            {
                XmlNode playerQuest = playerData.CreateElement("PlayerQuest");

                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = quest.Details.ID.ToString();
                playerQuest.Attributes.Append(idAttribute);

                XmlAttribute isCompletedAttribute = playerData.CreateAttribute("IsCompleted");
                isCompletedAttribute.Value = quest.IsCompleted.ToString();
                playerQuest.Attributes.Append(isCompletedAttribute);

                playerQuests.AppendChild(playerQuest);
            }

            return playerData.InnerXml; // XML document
        }

    }
}
