using Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdventureGame
{
    public partial class AdventureGameForm : Form
    {
        private Player player;
        private Enemy currentEnemy;
        private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";

        public AdventureGameForm()
        {
            InitializeComponent();

            if (File.Exists(PLAYER_DATA_FILE_NAME))
                player = Player.CreatePlayerFromXmlString(File.ReadAllText(PLAYER_DATA_FILE_NAME));
            else
                player = Player.CreateDefaultPlayer();

            //int ID = 1;
            //string Name = "Home";
            //string Description = "Home! Sweet Home!";
            //Location location = new Location(ID, Name, Description);

            //int CurrentHitPoints = 10;
            //int MaximumHitPoints = 10;
            //int Gold = 20;
            //int ExperiencePoints = 0;
            //int Level = 1;

            //player = new Player(CurrentHitPoints, MaximumHitPoints, Gold, ExperiencePoints);

            lblXPNeeded.Location = new Point(lblExperience.Right, lblExperience.Top);
            lblXPNeeded.Text = player.XPNeeded.ToString();
            label6.Visible = false;
            lblXPNeeded.Visible = false;

            lblHitPoints.DataBindings.Add("Text", player, "CurrentHitPoints");
            lblGold.DataBindings.Add("Text", player, "Gold");
            lblExperience.DataBindings.Add("Text", player, "ExperiencePoints");
            lblXPNeeded.DataBindings.Add("Text", player, "XPNeeded");
            lblLevel.DataBindings.Add("Text", player, "Level");

            //UpdatePlayerStats();

            //MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            MoveTo(player.CurrentLocation);

            //player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_BROKEN_SWORD), 1));

            
        }

        private void UpdatePlayerStats()
        {
            lblHitPoints.Text = player.CurrentHitPoints.ToString();
            lblGold.Text = player.Gold.ToString();
            string requiredXP = (10 * player.Level * player.Level + 90 * player.Level).ToString();
            lblExperience.Text = player.ExperiencePoints.ToString() + " / " + requiredXP;
            lblLevel.Text = player.Level.ToString();
        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(player.CurrentLocation.LocationToNorth);
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(player.CurrentLocation.LocationToSouth);
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            MoveTo(player.CurrentLocation.LocationToWest);
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            MoveTo(player.CurrentLocation.LocationToEast);
        }

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            // 2.1) Вземи оръжието, което е equipped 
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

            // 2.2) Определи damage, който ще се нанесе на Enemy
            int damageToEnemy = RandomNumberGenerator.NumberBetween(currentWeapon.MinimumDamage, currentWeapon.MaximumDamage);

            // 2.3) Приложи damage-a върху current HP на Enemy
            currentEnemy.CurrentHitPoints -= damageToEnemy;

            // 2.3.1) Изведи съобщение
            rtbMessages.Text += "You hit the " + currentEnemy.Name + " for " + damageToEnemy.ToString() + " points." + Environment.NewLine;

            // 2.4) Ако противникът е мъртъв (currentHP = 0)
            if(currentEnemy.CurrentHitPoints <= 0)
            {
                // 2.4.1) Съобщение за победен противник
                rtbMessages.Text += Environment.NewLine;
                rtbMessages.Text += "You defeated the " + currentEnemy.Name + Environment.NewLine;

                // 2.4.2) Добави xp на играча
                player.AddExperiencePoints(currentEnemy.RewardExperiencePoints);
                rtbMessages.Text += "You receive " + currentEnemy.RewardExperiencePoints.ToString() + " experience points" + Environment.NewLine;

                // 2.4.3) Добави gold на играча
                player.Gold += currentEnemy.RewardGold;
                rtbMessages.Text += "You receive " + currentEnemy.RewardGold.ToString() + " gold" + Environment.NewLine;

                // 2.4.4) Създай и отвори LootTable-a на мъртвия противник
                List<InventoryItem> lootedItems = new List<InventoryItem>();

                // Добави предмети към списъка, сравнявайки произволно число с %drop chance
                foreach(LootItem lootItem in currentEnemy.LootTable)
                    if(RandomNumberGenerator.NumberBetween(1, 100) <= lootItem.DropPercentage)
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));

                // Ако нито един предмет не "падне" (не се добави, възможно е ако няма предмети със 100% drop rate), да се добавят default items (може да е 1)
                if(lootedItems.Count == 0)
                    foreach(LootItem lootItem in currentEnemy.LootTable)
                        if(lootItem.IsDefaultItem)
                            lootedItems.Add(new InventoryItem(lootItem.Details, 1));

                // 2.4.4.1) и 2.4.4.2) Добави избрани items в Inventory на играта и изведи съобщение за всеки item от loot-a
                foreach (InventoryItem inventoryItem in lootedItems)
                {
                    player.AddItemToInventory(inventoryItem.Details);
                    if(inventoryItem.Quantity == 1)
                        rtbMessages.Text += "You loot " + inventoryItem.Quantity.ToString() + " " + inventoryItem.Details.Name +Environment.NewLine;
                    else
                        rtbMessages.Text += "You loot " + inventoryItem.Quantity.ToString() + " " + inventoryItem.Details.NamePlural + Environment.NewLine;
                }

                // 2.4.5) Обнови потребителския интерфейс
                //UpdatePlayerStats();

                UpdateInventoryListInUI();
                UpdateWeaponListInUI();
                UpdatePotionListInUI();

                // Нов ред в съобщенията
                rtbMessages.Text += Environment.NewLine;

                // 2.4.6) Премести героят на текущата локация (която е същата, но да няма enemy)
                MoveTo(player.CurrentLocation);
            }
            else
            {
                // 2.5) Противникът е жив

                // 2.5.1) Определи damage, който ще нанесе на player
                int damageToPlayer = RandomNumberGenerator.NumberBetween(0, currentEnemy.MaximumDamage);

                // 2.5.2) Изведи съобщение за damage-a
                rtbMessages.Text += "The " + currentEnemy.Name + " did " + damageToPlayer.ToString() + " points of damage." + Environment.NewLine;

                // 2.5.3) Извади damage-a от currentHP на играча
                player.CurrentHitPoints -= damageToPlayer;

                // 2.5.3.1) Обнови потребителския интерфейс да показва новите стойности на HP
                //lblHitPoints.Text = player.CurrentHitPoints.ToString();

                // 2.5.4) Ако играчът е мъртъв
                if(player.CurrentHitPoints <= 0)
                {
                    // 2.5.4.1) Съобщение 
                    rtbMessages.Text += "The " + currentEnemy.Name + " killed you." + Environment.NewLine;

                    // 2.5.4.2) Смени локацията на player на Home/Graveyard (само Home има; TODO: да направя Гробище за напред)
                    MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
                }
            }
        }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            // 3.1) Вземи текущия избран potion
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            // 3.2) Увеличи currentHP на играча
            player.CurrentHitPoints = (player.CurrentHitPoints + potion.AmountToHeal);

            // 3.2.1) currentHP НЕ трябва да надвишава maxHP
            if(player.CurrentHitPoints > player.MaximumHitPoints)
                player.CurrentHitPoints = player.MaximumHitPoints;

            // 3.3) Премахни използвания potion от Inventory
            foreach(InventoryItem ii in player.Inventory)
            {
                if(ii.Details.ID == potion.ID)
                {
                    ii.Quantity--;
                    break;
                }
            }

            // 3.4) Изведи съобщение
            rtbMessages.Text += "You drink a " + potion.Name + Environment.NewLine;

            // 3.5) Противникът получава ход да атакува

            // 3.5.1) Определи damage, който ще нанесе противникът на играча
            int damageToPlayer = RandomNumberGenerator.NumberBetween(0, currentEnemy.MaximumDamage);

            // 3.5.2) Изведи съобщение за damage-a
            rtbMessages.Text += "The " + currentEnemy.Name + " did " + damageToPlayer.ToString() + " points of damage." + Environment.NewLine;

            // 3.5.3) Извади damage-a от currentHP на играча
            player.CurrentHitPoints -= damageToPlayer;

            // 3.5.4) Ако играчът е мъртъв
            if(player.CurrentHitPoints <= 0)
            {
                // 3.5.4.1) Съобщение
                rtbMessages.Text += "The " + currentEnemy.Name + " killed you." + Environment.NewLine;

                // 3.5.4.2) Смени локацията на player на Home/Graveyard (само Home има; TODO: да направя Гробище за напред)
                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            }

            // 3.6) Обнови потребителския интерфейс
            //lblHitPoints.Text = player.CurrentHitPoints.ToString();
            UpdateInventoryListInUI();
            UpdatePotionListInUI();
        }

        private void MoveTo(Location newLocation)
        {
            // 1.1) Новото място има ли критерии (required items)?
            if (!player.HasRequiredItemToEnterThisLocation(newLocation))
            {
                    // 1.1.1) Играчът няма нужният предмет за преминаване, следователно извежда съобщение и прекъсва Moving процеса (излиза от MoveTo функцията)
                    rtbMessages.Text += "You must have a " + newLocation.ItemRequiredToEnter.Name + " to enter this location." + Environment.NewLine;
                    return;
            }
            // 1.1.2) Играчът има нужният предмет за преминаване щом не е минал през горния return

            // Обнови местоположението на играча
            player.CurrentLocation = newLocation;

            // Обнови възможностите за избор на посока за ново преместване
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnEast.Visible = (newLocation.LocationToEast != null);
            btnSouth.Visible = (newLocation.LocationToSouth != null);
            btnWest.Visible = (newLocation.LocationToWest != null);

            // Обнови текста за текущото местоположение и описание
            rtbLocation.Text = newLocation.Name + Environment.NewLine;
            rtbLocation.Text += newLocation.Description + Environment.NewLine;

            // 100% heal повреме на транспорта (героят се чувства отпочинал и зареден)
            player.CurrentHitPoints = player.MaximumHitPoints;

            // Отбележи промяната на HP на героя на екрана
            //lblHitPoints.Text = player.CurrentHitPoints.ToString();

            // 1.2) Тази нова локация има ли quest?
            if (newLocation.QuestAvailableHere != null)
            {
                // 1.2.1) Играчът притежава ли този quest

                //флаг дали играчът притежава quest-а
                bool playerAlreadyHasQuest = player.HasThisQuest(newLocation.QuestAvailableHere);
                //флаг дали играчът е завършил quest-a
                bool playerAlreadyCompletedQuest = player.CompletedThisQuest(newLocation.QuestAvailableHere);

                // Ако играчът притежава quest-a
                if (playerAlreadyHasQuest)
                {
                    // Играчът не е проверявал дали quest-a е завършен и все още е маркиран като незавършен
                    if (!playerAlreadyCompletedQuest)
                    {
                        // Флаг дали играчът притежава ВСИЧКИ (като бройка) quest items, нужни като критерии за завършването на quest-a
                        bool playerHasAllItemsToCompleteQuest = player.HasAllQuestCompletionItems(newLocation.QuestAvailableHere);

                        // 1.2.1.1.1.1) Играчът е изпълнил критериите за завършване на quest
                        if (playerHasAllItemsToCompleteQuest)
                        {
                            // 1.2.1.1.1.1.1) Изведи съобщение
                            rtbMessages.Text += Environment.NewLine;
                            rtbMessages.Text += "You completed the '" + newLocation.QuestAvailableHere.Name + "' quest." + Environment.NewLine;

                            // 1.2.1.1.1.1.2) Премахни quest items от Inventory на играча
                            player.RemoveQuestCompletionItems(newLocation.QuestAvailableHere);

                            // 1.2.1.1.1.1.3) Дай награда (увеличи gold, xp и евентуално добави Item/s в Inventory)
                            // Съобщение
                            rtbMessages.Text += "You receive: " + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() + " experience points" + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardGold.ToString() + " gold" + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardItem.Name + Environment.NewLine;
                            rtbMessages.Text += Environment.NewLine;

                            //xp/gold награда
                            player.AddExperiencePoints(newLocation.QuestAvailableHere.RewardExperiencePoints);
                            player.Gold += newLocation.QuestAvailableHere.RewardGold;
                            //UpdatePlayerStats();

                            // Добави Item награда, ако има такава (засега правя реализация със задължителен Item reward, примерно potion)
                            player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);

                            // 1.2.1.1.1.1.3) Маркирай quest-a като завършен (completed)
                            player.MarkQuestCompleted(newLocation.QuestAvailableHere);
                        }
                    }
                }
                else
                {
                    // 1.2.2) Играчът няма в quest log-a дадения quest

                    // 1.2.2.1) Изведи съобщение
                    rtbMessages.Text += "You receive the " + newLocation.QuestAvailableHere.Name + " quest." + Environment.NewLine;
                    rtbMessages.Text += newLocation.QuestAvailableHere.Description + Environment.NewLine;
                    rtbMessages.Text += "To complete it, return with:" + Environment.NewLine;
                    foreach (QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        if(qci.Quantity == 1)
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.Name + Environment.NewLine;
                        else
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.NamePlural + Environment.NewLine;
                    }
                    rtbMessages.Text += Environment.NewLine;

                    // 1.2.2.2) Добави го в quest log-a
                    player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
                }
            }

            // 1.3) Тази нова локация има ли enemy (enemies)?
            if (newLocation.EnemyLivingHere != null)
            {
                // 1.3.1.1) Съобщение
                rtbMessages.Text += "You see a " + newLocation.EnemyLivingHere.Name + Environment.NewLine;

                // 1.3.1.2) Създай противник, използвайки стойностите на стандартен противник от списъка с противници World.Enemies
                Enemy standardEnemy = World.EnemyByID(newLocation.EnemyLivingHere.ID);

                currentEnemy = new Enemy(standardEnemy.ID, standardEnemy.Name, standardEnemy.MaximumDamage, standardEnemy.RewardExperiencePoints, standardEnemy.RewardGold, standardEnemy.CurrentHitPoints, standardEnemy.MaximumHitPoints);

                foreach (LootItem lootItem in standardEnemy.LootTable)
                {
                    currentEnemy.LootTable.Add(lootItem);
                }

                // 1.3.1.3) Обнови потребителския интерфейс
                cboWeapons.Visible = true;
                cboPotions.Visible = true;
                btnUseWeapon.Visible = true;
                btnUsePotion.Visible = true;
            }
            else
            {
                // 1.3.2) Ако няма enemy
                currentEnemy = null;

                // 1.3.2.1) Скрий опциите за атака
                cboWeapons.Visible = false;
                cboPotions.Visible = false;
                btnUseWeapon.Visible = false;
                btnUsePotion.Visible = false;
            }

            // 1.4) Обнови Inventory
            UpdateInventoryListInUI();

            // 1.5) Oбнови quest log
            UpdateQuestListInUI();

            // 1.6) Oбнови списъка с оръжията и в момента equipped (weapons' combobox)
            UpdateWeaponListInUI();

            // 1.7) Oбнови списъка с Potions
            UpdatePotionListInUI();
        }

        private void UpdateInventoryListInUI()
        {
            dgvInventory.RowHeadersVisible = false;

            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Name";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Quantity";

            dgvInventory.Rows.Clear();

            foreach (InventoryItem inventoryItem in player.Inventory)
            {
                if (inventoryItem.Quantity > 0)
                {
                    dgvInventory.Rows.Add(new[] { inventoryItem.Details.Name, inventoryItem.Quantity.ToString() });
                }
            }
        }

        private void UpdateQuestListInUI()
        {
            dgvQuests.RowHeadersVisible = false;

            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Name";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Done?";

            dgvQuests.Rows.Clear();

            foreach (PlayerQuest playerQuest in player.Quests)
            {
                dgvQuests.Rows.Add(new[] { playerQuest.Details.Name, playerQuest.IsCompleted.ToString() });
            }
        }

        private void UpdateWeaponListInUI()
        {
            List<Weapon> weapons = new List<Weapon>();

            foreach (InventoryItem inventoryItem in player.Inventory)
                if (inventoryItem.Details is Weapon)
                    if (inventoryItem.Quantity > 0)
                        weapons.Add((Weapon)inventoryItem.Details);

            if (weapons.Count == 0)
            {
                // 1.6.1) Играчът няма никакви оръжия, следователно скрий списъка с оръжия и бутона Use за използване на оръжие
                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;
            }
            else
            {
                // 1.6.2) Визуализирай обновения списък с оръжия
                cboWeapons.SelectedIndexChanged -= cboWeapons_SelectedIndexChanged; // функцията вдясно се случва автоматично, но не искаме това да стане преди set-ването на DataSource property-то, затова с този ред "изключваме" автоматичното викане на void функцията вдясно, която променихме да прави, каквото на нас ни е нужно
                cboWeapons.DataSource = weapons;
                cboWeapons.SelectedIndexChanged += cboWeapons_SelectedIndexChanged; // сега вече е нужно да "прикрепим" отново функцията да се "случва" винаги когато се "случи" събитието SelectedIndexChanged, тоест при смяна на избрано оръжие, след като е променен DataSource, да се сменя текущото оръжие с избраното по default, за да се реализира и в обратния случай - примерно при смяна на локация падащият списък да е избрал автоматично текущото оръжие (на даден индекс), а не винаги най-горният
                // -= disconnect-ва функция към event, а += connect-ва отново функцията към event-a
                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";

                if (player.CurrentWeapon != null)
                    cboWeapons.SelectedItem = player.CurrentWeapon;
                else
                    cboWeapons.SelectedIndex = 0;
            }
        }

        private void UpdatePotionListInUI()
        {
            List<HealingPotion> healingPotions = new List<HealingPotion>();

            foreach (InventoryItem inventoryItem in player.Inventory)
            {
                if (inventoryItem.Details is HealingPotion)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        healingPotions.Add((HealingPotion)inventoryItem.Details);
                    }
                }
            }

            if (healingPotions.Count == 0)
            {
                // 1.7.1) Играчът няма никакви potions, следователно скрий списъка с potions и бутона Use за използване на potion
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
            }
            else
            {
                // 1.7.2) Визуализирай обновения списък с potions
                cboPotions.DataSource = healingPotions;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";

                cboPotions.SelectedIndex = 0;
            }
        }

        private void ScrollToBottomOfMessages()
        {
            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.ScrollToCaret();
        }

        private void rtbMessages_TextChanged(object sender, EventArgs e)
        {
            ScrollToBottomOfMessages();
        }

        private void AdventureGameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, player.ToXmlString());
        }

        private void cboWeapons_SelectedIndexChanged(object sender, EventArgs e)
        {
            player.CurrentWeapon = (Weapon)cboWeapons.SelectedItem;
        }

        private void btnMinimap_Click(object sender, EventArgs e)
        {
            MinimapScreen minimapScreen = new MinimapScreen(player);
            minimapScreen.StartPosition = FormStartPosition.CenterParent;
            minimapScreen.ShowDialog(this);
        }

        private void UpdateXPUI()
        {
            label6.Visible = true;
            lblXPNeeded.Visible = true;
            lblXPNeeded.Location = new Point(lblExperience.Right + 30, lblExperience.Top);
            label6.Location = new Point(lblXPNeeded.Right, lblXPNeeded.Top);
        }

        private void lblXPNeeded_TextChanged(object sender, EventArgs e)
        {
            UpdateXPUI();
        }

        private void AdventureGameForm_Load(object sender, EventArgs e)
        {
            UpdateXPUI();
        }
    }
}
