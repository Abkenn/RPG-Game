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

            // Update Player Stats in UI

            lblXPNeeded.Location = new Point(lblExperience.Right, lblExperience.Top);
            lblXPNeeded.Text = player.XPNeeded.ToString();
            label6.Visible = false;
            lblXPNeeded.Visible = false;

            lblHitPoints.DataBindings.Add("Text", player, "CurrentHitPoints");
            lblGold.DataBindings.Add("Text", player, "Gold");
            lblExperience.DataBindings.Add("Text", player, "ExperiencePoints");
            lblXPNeeded.DataBindings.Add("Text", player, "XPNeeded");
            lblLevel.DataBindings.Add("Text", player, "Level");

            // Update the Inventory in UI

            dgvInventory.RowHeadersVisible = false;
            dgvInventory.AutoGenerateColumns = false;

            dgvInventory.DataSource = player.Inventory;

            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 197,
                DataPropertyName = "Description"
            });

            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Quantity",
                DataPropertyName = "Quantity"
            });

            // Update Quest List in UI

            dgvQuests.RowHeadersVisible = false;
            dgvQuests.AutoGenerateColumns = false;

            dgvQuests.DataSource = player.Quests;

            dgvQuests.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 197,
                DataPropertyName = "Name"
            });

            dgvQuests.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Done?",
                DataPropertyName = "IsCompleted"
            });

            // Bind the comboboxes

            cboWeapons.DataSource = player.Weapons;
            cboWeapons.DisplayMember = "Name";
            cboWeapons.ValueMember = "Id";

            if (player.CurrentWeapon != null)
                cboWeapons.SelectedItem = player.CurrentWeapon;

            cboWeapons.SelectedIndexChanged += cboWeapons_SelectedIndexChanged; // прикрепяме при смяна на оръжие да се променя текущия избран weapon от комбобокса

            cboPotions.DataSource = player.Potions;
            cboPotions.DisplayMember = "Name";
            cboPotions.ValueMember = "Id";

            player.PropertyChanged += PlayerOnPropertyChanged;
            player.OnMessage += DisplayMessage;


            //MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            player.MoveTo(player.CurrentLocation);

            //player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_BROKEN_SWORD), 1));


        }

        private void DisplayMessage(object sender, MessageEventArgs messageEventArgs)
        {
            rtbMessages.Text += messageEventArgs.Message + Environment.NewLine;

            if (messageEventArgs.AddExtraNewLine)
                rtbMessages.Text += Environment.NewLine;

            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.ScrollToCaret();
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
            player.MoveTo(player.CurrentLocation.LocationToNorth);
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            player.MoveTo(player.CurrentLocation.LocationToSouth);
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            player.MoveTo(player.CurrentLocation.LocationToWest);
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            player.MoveTo(player.CurrentLocation.LocationToEast);
        }

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            // 2.1) Вземи оръжието, което е equipped 
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;
            player.UseWeapon(currentWeapon);
        }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            // 3.1) Вземи текущия избран potion
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;
            player.UsePotion(potion);
        }

        private void PlayerOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Weapons") // проверява кое property от player е било променено и проверява в случая първо за weapons, после за potions
            {
                cboWeapons.DataSource = player.Weapons;

                if (!player.Weapons.Any())
                {
                    cboWeapons.Visible = false;
                    btnUseWeapon.Visible = false;
                }
            }

            if (propertyChangedEventArgs.PropertyName == "Potions")
            {
                cboPotions.DataSource = player.Potions;

                if (!player.Potions.Any())
                {
                    cboPotions.Visible = false;
                    btnUsePotion.Visible = false;
                }
            }
            if (propertyChangedEventArgs.PropertyName == "CurrentLocation")
            {
                // Възможни локации
                btnNorth.Visible = (player.CurrentLocation.LocationToNorth != null);
                btnEast.Visible = (player.CurrentLocation.LocationToEast != null);
                btnSouth.Visible = (player.CurrentLocation.LocationToSouth != null);
                btnWest.Visible = (player.CurrentLocation.LocationToWest != null);

                // Изведи текуща локация и описание
                rtbLocation.Text = player.CurrentLocation.Name + Environment.NewLine;
                rtbLocation.Text += player.CurrentLocation.Description + Environment.NewLine;

                if (player.CurrentLocation.EnemyLivingHere == null)
                {
                    cboWeapons.Visible = false;
                    cboPotions.Visible = false;
                    btnUseWeapon.Visible = false;
                    btnUsePotion.Visible = false;
                }
                else
                {
                    cboWeapons.Visible = player.Weapons.Any();
                    cboPotions.Visible = player.Potions.Any();
                    btnUseWeapon.Visible = player.Weapons.Any();
                    btnUsePotion.Visible = player.Potions.Any();
                }
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
