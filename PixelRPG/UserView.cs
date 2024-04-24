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
            menu.StartGame += () => StartGame();
            SizeChanged += (sender, e) => { menu.ChangeButtonTextSize(ClientSize.Height*ButtonBasedTextSize/300); };
            Controls.Add(menu.MenuTable);
        }

        public void StartGame()
        {
            game = new GameModel(40);
            visual = new GameVisual(game);
            controls = new GameControls(game, visual);
            Controls.Clear();
            var tableView = visual.GetWorldVisual(game.Player.Position);
            var table = SetImages(SetGameTable(), tableView);
            visual.ChangeWorldCellView += (row, column, worldCell) =>
            {
                var image = Image.FromFile(game.FileName(worldCell));
                var pict = (PictureBox)table.GetControlFromPosition(column, row);
                ((PictureBox)table.GetControlFromPosition(row,column)).Image = image;
                ((PictureBox)table.GetControlFromPosition(row, column)).SizeMode = PictureBoxSizeMode.Zoom;
            };
            visual.ChangePlayerAvatarView += (row, column, character) =>
            {
                var image = Image.FromFile(game.FileName(character));
                var pict = (PictureBox)table.GetControlFromPosition(column, row);
                ((PictureBox)table.GetControlFromPosition(row, column)).Image = image;
                ((PictureBox)table.GetControlFromPosition(row, column)).SizeMode = PictureBoxSizeMode.Zoom;
            };
            var keyBar = new TextBox();
            controls.SetKeyCommands(keyBar);
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