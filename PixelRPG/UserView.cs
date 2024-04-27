using System.Data.Common;
using System.Security.Cryptography.X509Certificates;

namespace PixelRPG
{
    public class UserView : Form
    {
        private GameModel game;
        private GameVisual visual;
        private GameControls controls;
        private Menu menu;
        public const int ButtonBasedTextSize = 15;
        public const string ButtonBasedFontFamily = "Arial";
        public readonly Color BaseWorldColor = Color.FromArgb(119,185,129);
        public UserView()
        {
            menu = new Menu();
            var keyBar = new TextBox() { Size = new Size(0,0)};
            menu.SetControls(keyBar);
            menu.StartGame += () => StartGame();
            menu.CloseForm += () => Close();
            SizeChanged += (sender, e) =>  menu.ChangeButtonTextSize(ClientSize.Height*ButtonBasedTextSize/300); 
            Controls.Add(keyBar);
            Controls.Add(menu.MenuTable);
            keyBar.Focus();
            keyBar.Select();
        }

        public void StartGame()
        {
            Controls.Clear();
            game = new GameModel(40);
            visual = new GameVisual(game);
            controls = new GameControls(game, visual);
            var tableView = visual.GetWorldVisual(game.Player.Position);
            var table = SetImages(SetGameTable(), tableView);
            var keyBar = new TextBox();
            controls.SetKeyCommands(keyBar);
            visual.ChangeOneCellView += (row, column, worldCell,player) =>
            {
                var image = player==null? Image.FromFile(game.FileName(worldCell)): Image.FromFile(game.FileName(player));
                var pict = (PictureBox)table.GetControlFromPosition(row, column);
                pict.Image = image;
                pict.SizeMode = PictureBoxSizeMode.Zoom;
            };
            Controls.Add(table);
            Controls.Add(keyBar);
            keyBar.Focus();
            keyBar.Select();
        }

        public TableLayoutPanel SetGameTable()
        {
            var table = new TableLayoutPanel() { BackColor = BaseWorldColor, Dock = DockStyle.Fill };
            for (int i = 0; i < GameVisual.ViewFieldSize; i++)
            {
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / GameVisual.ViewFieldSize));
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / GameVisual.ViewFieldSize));
            }
            return table;
        }

        public TableLayoutPanel SetImages(TableLayoutPanel table, WorldElement[,] tableView)
        {
            for (int i = 0; i < GameVisual.ViewFieldSize; i++)
                for (int j = 0; j < GameVisual.ViewFieldSize; j++)
                {
                    var image = Image.FromFile(game.FileName(tableView[i, j]));
                    var p = new PictureBox() { Dock = DockStyle.Fill, Image = image };
                    p.SizeMode = PictureBoxSizeMode.Zoom;
                    table.Controls.Add(p, i, j);
                }
            var playerImage = Image.FromFile(game.FileName(game.Player));
            ((PictureBox)table.GetControlFromPosition(GameVisual.ViewFieldSize/2, GameVisual.ViewFieldSize/2)).Image = playerImage;
            return table;
        }
    }
}