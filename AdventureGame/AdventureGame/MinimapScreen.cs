using Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdventureGame
{
    public partial class MinimapScreen : Form
    {
        private Player currentPlayer;
        public MinimapScreen(Player player)
        {
            InitializeComponent();

            currentPlayer = player;

            this.FormBorderStyle = FormBorderStyle.Fixed3D;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
        }

        private void MinimapScreen_Load(object sender, EventArgs e)
        {
            Image image = currentPlayer.CurrentLocation.Minimap;
            pictureBox1.Image = image;
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;

            this.MinimumSize = new Size(143, 115);
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.Opacity = 0.7;
        }
    }
}
