using System.Data.Common;
using System.Security.Cryptography.X509Certificates;

namespace PixelRPG
{
    public partial class UserView : Form
    {
        private GameModel game;
        private GameVisual visual;
        private GameControls controls;
        public const int ButtonBasedTextSize = 15;
        public const string ButtonBasedFontFamily = "Arial";
        public readonly Color BaseWorldColor = Color.FromArgb(119,185,129);
        public UserView()
        {
            var menu = new Menu();
            menu.StartGame += menuTable =>  StartGame(menuTable);
            SizeChanged += (sender, e) => { menu.ChangeButtonTextSize(ClientSize.Height*ButtonBasedTextSize/300); };
            Controls.Add(menu.MenuTable);
        }

        public void StartGame(TableLayoutPanel menuTable)
        {
            game = new GameModel(40);
            visual = new GameVisual(game);
            controls = new GameControls(game, visual);
            Controls.Remove(menuTable);
            var table = new TableLayoutPanel() {BackColor=BaseWorldColor , Dock=DockStyle.Fill};
            SetGameTable(table);
            var tableView = visual.GetWorldVisual(game.PlayerPosition);
            SetImages(table, tableView);
            visual.ChangeWorldCellView += (row, column, worldCell) =>
            {
                var image = Image.FromFile(game.FileName(worldCell));
                var pict = (PictureBox)table.GetControlFromPosition(column, row);
                ((PictureBox)table.GetControlFromPosition(column, row)).Image = image;
                ((PictureBox)table.GetControlFromPosition(column, row)).SizeMode = PictureBoxSizeMode.Zoom;
            };
            visual.ChangePlayerAvatarView += (row, column, character) =>
            {
                var image = Image.FromFile(game.FileName(character));
                var pict = (PictureBox)table.GetControlFromPosition(column, row);
                ((PictureBox)table.GetControlFromPosition(column, row)).Image = image;
                ((PictureBox)table.GetControlFromPosition(column, row)).SizeMode = PictureBoxSizeMode.Zoom;
            };
            var keyBar = new TextBox();
            controls.SetKeyCommands(keyBar);
            Controls.Add(table);
            Controls.Add(keyBar);
            keyBar.Focus();
            keyBar.Select();
        }

        public void SetGameTable(TableLayoutPanel table)
        {
            for (int i = 0; i < GameVisual.ViewFieldSize; i++)
            {
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / GameVisual.ViewFieldSize));
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / GameVisual.ViewFieldSize));
            }
        }

        public void SetImages(TableLayoutPanel table, WorldCell[,] tableView)
        {
            for (int i = 0; i < GameVisual.ViewFieldSize; i++)
                for (int j = 0; j < GameVisual.ViewFieldSize; j++)
                {
                    var image = Image.FromFile(game.FileName(tableView[i, j]));
                    var p = new PictureBox() { Dock = DockStyle.Fill, Image = image };
                    p.SizeMode = PictureBoxSizeMode.Zoom;
                    table.Controls.Add(p, j, i);
                }
            var playerImage = Image.FromFile(game.FileName(game.PlayerView));
            ((PictureBox)table.GetControlFromPosition(GameVisual.ViewFieldSize/2, GameVisual.ViewFieldSize/2)).Image = playerImage;
        }
    }
}