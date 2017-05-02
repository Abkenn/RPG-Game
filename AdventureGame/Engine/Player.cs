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
        public int Level { get { return (int)Math.Floor(((-90 + Math.Sqrt(8100 + 40 * ExperiencePoints)) / 20 + 1)); } } // xp = 10level^2 + 90level е формулата, ама +1 level, за да не почва от 0

        private Location currentLocation;
        public Location CurrentLocation
        {
            get { return currentLocation; }
            set
            {
                currentLocation = value;
                OnPropertyChanged("CurrentLocation");
            }
        }

        private Enemy currentEnemy;

        public Weapon CurrentWeapon { get; set; }
        public BindingList<InventoryItem> Inventory { get; set; }
        public BindingList<PlayerQuest> Quests { get; set; }
        public List<Weapon> Weapons { get { return Inventory.Where(x => x.Details is Weapon).Select(x => x.Details as Weapon).ToList(); } }
        public List<HealingPotion> Potions { get { return Inventory.Where(x => x.Details is HealingPotion).Select(x => x.Details as HealingPotion).ToList(); } }

        public event EventHandler<MessageEventArgs> OnMessage;


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

        private void RaiseMessage(string message, bool addExtraNewLine = false)
        {
            OnMessage?.Invoke(this, new MessageEventArgs(message, addExtraNewLine));
        }

        public void MoveTo(Location newLocation)
        {
            // 1.1) Новото място има ли критерии (required items)?
            if (!HasRequiredItemToEnterThisLocation(newLocation))
            {
                // 1.1.1) Играчът няма нужният предмет за преминаване, следователно извежда съобщение и прекъсва Moving процеса (излиза от MoveTo функцията)
                //rtbMessages.Text += "You must have a " + newLocation.ItemRequiredToEnter.Name + " to enter this location." + Environment.NewLine;
                RaiseMessage("You must have a " + newLocation.ItemRequiredToEnter.Name + " to enter this location.");
                return;
            }
            // 1.1.2) Играчът има нужният предмет за преминаване щом не е минал през горния return

            // Обнови местоположението на играча
            CurrentLocation = newLocation;

            // 100% heal повреме на транспорта (героят се чувства отпочинал и зареден)
            CurrentHitPoints = MaximumHitPoints;

            // Отбележи промяната на HP на героя на екрана
            //lblHitPoints.Text = player.CurrentHitPoints.ToString();

            // 1.2) Тази нова локация има ли quest?
            if (newLocation.QuestAvailableHere != null)
            {
                // 1.2.1) Играчът притежава ли този quest

                //флаг дали играчът притежава quest-а
                bool playerAlreadyHasQuest = HasThisQuest(newLocation.QuestAvailableHere);
                //флаг дали играчът е завършил quest-a
                bool playerAlreadyCompletedQuest = CompletedThisQuest(newLocation.QuestAvailableHere);

                // Ако играчът притежава quest-a
                if (playerAlreadyHasQuest)
                {
                    // Играчът не е проверявал дали quest-a е завършен и все още е маркиран като незавършен
                    if (!playerAlreadyCompletedQuest)
                    {
                        // Флаг дали играчът притежава ВСИЧКИ (като бройка) quest items, нужни като критерии за завършването на quest-a
                        bool playerHasAllItemsToCompleteQuest = HasAllQuestCompletionItems(newLocation.QuestAvailableHere);

                        // 1.2.1.1.1.1) Играчът е изпълнил критериите за завършване на quest
                        if (playerHasAllItemsToCompleteQuest)
                        {
                            // 1.2.1.1.1.1.1) Изведи съобщение
                            RaiseMessage(""); // за да добави нов ред
                            RaiseMessage("You complete the '" + newLocation.QuestAvailableHere.Name + "' quest.");

                            // 1.2.1.1.1.1.2) Премахни quest items от Inventory на играча
                            RemoveQuestCompletionItems(newLocation.QuestAvailableHere);

                            // 1.2.1.1.1.1.3) Дай награда (увеличи gold, xp и евентуално добави Item/s в Inventory)
                            // Съобщение
                            RaiseMessage("You receive: ");
                            RaiseMessage(newLocation.QuestAvailableHere.RewardExperiencePoints + " experience points");
                            RaiseMessage(newLocation.QuestAvailableHere.RewardGold + " gold");
                            RaiseMessage(newLocation.QuestAvailableHere.RewardItem.Name, true); // addExtraNewLine=true, за да добави допълнителен празен ред накрая

                            //xp/gold награда
                            AddExperiencePoints(newLocation.QuestAvailableHere.RewardExperiencePoints);
                            Gold += newLocation.QuestAvailableHere.RewardGold;
                            //UpdatePlayerStats();

                            // Добави Item награда, ако има такава (засега правя реализация със задължителен Item reward, примерно potion)
                            AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);

                            // 1.2.1.1.1.1.3) Маркирай quest-a като завършен (completed)
                            MarkQuestCompleted(newLocation.QuestAvailableHere);
                        }
                    }
                }
                else
                {
                    // 1.2.2) Играчът няма в quest log-a дадения quest

                    // 1.2.2.1) Изведи съобщение
                    RaiseMessage("You receive the " + newLocation.QuestAvailableHere.Name + " quest.");
                    RaiseMessage(newLocation.QuestAvailableHere.Description);
                    RaiseMessage("To complete it, return with:");
                    foreach (QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        if (qci.Quantity == 1)
                            RaiseMessage(qci.Quantity + " " + qci.Details.Name);
                        else
                            RaiseMessage(qci.Quantity + " " + qci.Details.NamePlural);
                    }
                    RaiseMessage("");

                    // 1.2.2.2) Добави го в quest log-a
                    Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
                }
            }

            // 1.3) Тази нова локация има ли enemy (enemies)?
            if (newLocation.EnemyLivingHere != null)
            {
                // 1.3.1.1) Съобщение
                RaiseMessage("You see a " + newLocation.EnemyLivingHere.Name);

                // 1.3.1.2) Създай противник, използвайки стойностите на стандартен противник от списъка с противници World.Enemies
                Enemy standardEnemy = World.EnemyByID(newLocation.EnemyLivingHere.ID);

                currentEnemy = new Enemy(standardEnemy.ID, standardEnemy.Name, standardEnemy.MaximumDamage, standardEnemy.RewardExperiencePoints, standardEnemy.RewardGold, standardEnemy.CurrentHitPoints, standardEnemy.MaximumHitPoints);

                foreach (LootItem lootItem in standardEnemy.LootTable)
                    currentEnemy.LootTable.Add(lootItem);

                // 1.3.1.3) Обнови потребителския интерфейс
                // автоматично с propertychanged event
            }
            else
            {
                // 1.3.2) Ако няма enemy
                currentEnemy = null;

                // 1.3.2.1) Скрий опциите за атака
                // автоматично
            }

            // 1.4) Обнови Inventory
            //UpdateInventoryListInUI();

            // 1.5) Oбнови quest log
            //UpdateQuestListInUI();

            // 1.6) Oбнови списъка с оръжията и в момента equipped (weapons' combobox)
            //UpdateWeaponListInUI();

            // 1.7) Oбнови списъка с Potions
            //UpdatePotionListInUI();
        }

        public void UseWeapon(Weapon weapon)
        {
            // 2.2) Определи damage, който ще се нанесе на Enemy
            int damageToEnemy = RandomNumberGenerator.NumberBetween(weapon.MinimumDamage, weapon.MaximumDamage);

            // 2.3) Приложи damage-a върху current HP на Enemy
            currentEnemy.CurrentHitPoints -= damageToEnemy;

            // 2.3.1) Изведи съобщение
            RaiseMessage("You hit the " + currentEnemy.Name + " for " + damageToEnemy + " points.");

            // 2.4) Ако противникът е мъртъв (currentHP = 0)
            if (currentEnemy.CurrentHitPoints <= 0)
            {
                // 2.4.1) Съобщение за победен противник
                RaiseMessage("");
                RaiseMessage("You defeated the " + currentEnemy.Name);

                // 2.4.2) Добави xp на играча
                AddExperiencePoints(currentEnemy.RewardExperiencePoints);
                RaiseMessage("You receive " + currentEnemy.RewardExperiencePoints + " experience points");

                // 2.4.3) Добави gold на играча
                Gold += currentEnemy.RewardGold;
                RaiseMessage("You receive " + currentEnemy.RewardGold + " gold");

                // 2.4.4) Създай и отвори LootTable-a на мъртвия противник
                List<InventoryItem> lootedItems = new List<InventoryItem>();

                // Добави предмети към списъка, сравнявайки произволно число с %drop chance
                foreach (LootItem lootItem in currentEnemy.LootTable)
                    if (RandomNumberGenerator.NumberBetween(1, 100) <= lootItem.DropPercentage)
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));

                // Ако нито един предмет не "падне" (не се добави, възможно е ако няма предмети със 100% drop rate), да се добавят default items (може да е 1)
                if (lootedItems.Count == 0)
                    foreach (LootItem lootItem in currentEnemy.LootTable)
                        if (lootItem.IsDefaultItem)
                            lootedItems.Add(new InventoryItem(lootItem.Details, 1));

                // 2.4.4.1) и 2.4.4.2) Добави избрани items в Inventory на играта и изведи съобщение за всеки item от loot-a
                foreach (InventoryItem inventoryItem in lootedItems)
                {
                    AddItemToInventory(inventoryItem.Details);
                    if (inventoryItem.Quantity == 1)
                        RaiseMessage("You loot " + inventoryItem.Quantity + " " + inventoryItem.Details.Name);
                    else
                        RaiseMessage("You loot " + inventoryItem.Quantity + " " + inventoryItem.Details.NamePlural);
                }

                // 2.4.5) Обнови потребителския интерфейс
                //UpdatePlayerStats();

                //UpdateInventoryListInUI();
                //UpdateWeaponListInUI();
                //UpdatePotionListInUI();

                // Нов ред в съобщенията
                RaiseMessage("");

                // 2.4.6) Премести героят на текущата локация (която е същата, но да няма enemy)
                MoveTo(CurrentLocation);
            }
            else
            {
                // 2.5) Противникът е жив

                // 2.5.1) Определи damage, който ще нанесе на player
                int damageToPlayer = RandomNumberGenerator.NumberBetween(0, currentEnemy.MaximumDamage);

                // 2.5.2) Изведи съобщение за damage-a
                RaiseMessage("The " + currentEnemy.Name + " did " + damageToPlayer + " points of damage.");

                // 2.5.3) Извади damage-a от currentHP на играча
                CurrentHitPoints -= damageToPlayer;

                // 2.5.3.1) Обнови потребителския интерфейс да показва новите стойности на HP
                //lblHitPoints.Text = player.CurrentHitPoints.ToString();

                // 2.5.4) Ако играчът е мъртъв
                if (CurrentHitPoints <= 0)
                {
                    // 2.5.4.1) Съобщение 
                    RaiseMessage("The " + currentEnemy.Name + " killed you.");

                    // 2.5.4.2) Смени локацията на player на Home/Graveyard (само Home има; TODO: да направя Гробище за напред)
                    MoveToHome();
                }
            }
        }

        public void UsePotion(HealingPotion potion)
        {

            // 3.2) Увеличи currentHP на играча
            CurrentHitPoints = (CurrentHitPoints + potion.AmountToHeal);

            // 3.2.1) currentHP НЕ трябва да надвишава maxHP
            if (CurrentHitPoints > MaximumHitPoints)
                CurrentHitPoints = MaximumHitPoints;

            // 3.3) Премахни използвания potion от Inventory
            RemoveItemFromInventory(potion, 1);

            // 3.4) Изведи съобщение
            RaiseMessage("You drink a " + potion.Name);

            // 3.5) Противникът получава ход да атакува

            // 3.5.1) Определи damage, който ще нанесе противникът на играча
            int damageToPlayer = RandomNumberGenerator.NumberBetween(0, currentEnemy.MaximumDamage);

            // 3.5.2) Изведи съобщение за damage-a
            RaiseMessage("The " + currentEnemy.Name + " did " + damageToPlayer + " points of damage.");

            // 3.5.3) Извади damage-a от currentHP на играча
            CurrentHitPoints -= damageToPlayer;

            // 3.5.4) Ако играчът е мъртъв
            if (CurrentHitPoints <= 0)
            {
                // 3.5.4.1) Съобщение
                RaiseMessage("The " + currentEnemy.Name + " killed you.");

                // 3.5.4.2) Смени локацията на player на Home/Graveyard (само Home има; TODO: да направя Гробище за напред)
                MoveToHome();
            }

            // 3.6) Обнови потребителския интерфейс
            //lblHitPoints.Text = player.CurrentHitPoints.ToString();
            //UpdateInventoryListInUI();
            //UpdatePotionListInUI();
        }

        private void MoveToHome()
        {
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
        }

        public void AddExperiencePoints(int experiencePointsToAdd)
        {
            ExperiencePoints += experiencePointsToAdd;
            MaximumHitPoints = 2 * (Level * Level + 2 * Level - 1);
            switch (Level)
            {
                case 1:
                    MaximumHitPoints = 10;
                    break;
                case 2:
                    MaximumHitPoints = 21;
                    break;
                case 3:
                    MaximumHitPoints = 32;
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
                    RemoveItemFromInventory(item.Details, qci.Quantity);
            }
        }

        public void AddItemToInventory(Item itemToAdd, int quantity = 1)
        {
            // Обхожда Inventory, за да провери дали reward item-a го няма вече в Inventory
            InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToAdd.ID);

            if (item == null) // RewardItem не се съдържа в Inventory щом все още сме във функцията, така че добави reward item-a като нов InventoryItem (в нов слот) с брой 1 по default
                Inventory.Add(new InventoryItem(itemToAdd, quantity));
            else // RewardItem се съдържа в Inventory, затова само увеличи броя с 1
                item.Quantity++;
            RaiseInventoryChangedEvent(itemToAdd);
        }

        public void RemoveItemFromInventory(Item itemToRemove, int quantity = 1)
        {
            InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToRemove.ID);

            if (item == null)
            {
                //TODO: тук трябва да се изведе грешка, че избраният предмет не е в Inventory (нито potion, нито weapon)
            }
            else
            {
                // Предметът се съдържа в Inventory, намали Quantity с избрано quantity
                item.Quantity -= quantity;

                // Не трябва да имаме отрицателно количество
                if (item.Quantity < 0)
                {
                    //TODO: тук отново трябва да се изведе грешка
                    item.Quantity = 0;
                }

                // А ако количеството е 0, трябва да се премахне дадения предмет от "слота"
                if (item.Quantity == 0)
                    Inventory.Remove(item);

                // Извикай функцията, която ще изпрати notification да се обнови екрана за промяна в Inventory (Weapons или Potions)
                RaiseInventoryChangedEvent(itemToRemove);
            }
        }

        private void RaiseInventoryChangedEvent(Item item)
        {
            if (item is Weapon)
                OnPropertyChanged("Weapons");
            if (item is HealingPotion)
                OnPropertyChanged("Potions");
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
