using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Player : LivingCreature
    {
        public int Gold { get; set; }
        public int ExperiencePoints { get; set; }
        public int Level { get; set; }
        public List<InventoryItem> Inventory { get; set; }
        public List<PlayerQuest> Quests { get; set; }
        public Location CurrentLocation { get; set; }

        public Player(int currentHP, int maximumHP, int gold, int xp, int level) : base(currentHP, maximumHP)
        {
            Gold = gold;
            ExperiencePoints = xp;
            Level = level;

            Inventory = new List<InventoryItem>();
            Quests = new List<PlayerQuest>();
        }

        public bool HasRequiredItemToEnterThisLocation(Location location)
        {
            if (location.ItemRequiredToEnter == null)
            {
                // Няма required items за тази локация, така че излез с true
                return true;
            }
            foreach (InventoryItem ii in Inventory)
            {
                if (ii.Details.ID == location.ItemRequiredToEnter.ID)
                {
                    // Намерен е предмет от Inventory, който е RequiredToEnter за дадената локация
                    return true;
                }
            }

            // 1.1.1) Играчът няма нужният предмет за преминаване, при такъв изход е нужно съобщение 
            return false;
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
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                bool foundItemInPlayersInventory = false;
                // Обхожда цялото Inventory, за да провери дали играчът притежава quest item и дали има нужната бройка да завърши quest-a
                foreach (InventoryItem ii in Inventory)
                {
                    if (ii.Details.ID == qci.Details.ID) // Играчът има нужният предмет в Inventory?
                    {
                        foundItemInPlayersInventory = true;
                        if (ii.Quantity < qci.Quantity) // Играчът не притежава ДОСТАТЪЧЕН брой от нужния quest item
                            return false;
                    }
                }
                // Играчът не притежава изобщо от исканият item
                if (!foundItemInPlayersInventory)
                    return false;
            }

            // Щом все още не сме излезли от функцията, значи играчът има нужният item и достатъчна бройка от него, за да приключи Quest-a
            return true;
        }

        public void RemoveQuestCompletionItems(Quest quest)
        {
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                foreach (InventoryItem ii in Inventory)
                {
                    if (ii.Details.ID == qci.Details.ID)
                    {
                        // Извади нужния брой quest items от броя им в Inventory 
                        ii.Quantity -= qci.Quantity;
                        break;
                    }
                }
            }
        }

        public void AddItemToInventory(Item itemToAdd)
        {
            // Обхожда Inventory, за да провери дали reward item-a го няма вече в Inventory
            foreach (InventoryItem ii in Inventory)
            {
                if (ii.Details.ID == itemToAdd.ID)
                {
                    // RewardItem се съдържа в Inventory, затова само увеличи броя с 1
                    ii.Quantity++;

                    return; //добавен е RewardItem и излез
                }
            }

            // RewardItem не се съдържа в Inventory щом все още сме във функцията, така че добави reward item-a като нов InventoryItem (в нов слот) с брой 1
            Inventory.Add(new InventoryItem(itemToAdd, 1));
        }

        public void MarkQuestCompleted(Quest quest)
        {
            // Обходи списъка с quests и намери quest-ът, който току що завърши
            foreach (PlayerQuest pq in Quests)
            {
                if (pq.Details.ID == quest.ID)
                {
                    // Отбележи го като completed
                    pq.IsCompleted = true;

                    return; // Намерили сме quest-a и всичко е наред, няма смисъл от по-нататъшна проверка на другите quests
                }
            }
        }
    }
}
