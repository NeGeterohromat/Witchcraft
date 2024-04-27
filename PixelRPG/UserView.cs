using System.Data.Common;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace PixelRPG
{
    public class UserView : Form
    {
        private GameModel game;
        private GameVisual visual;
        private GameControls controls;
        private Menu menu;
        private TableLayoutPanel lastInventoryView;
        private TableLayoutPanel gameView;
        private PictureBox firstSelectedSlotInInventory;
        private PictureBox secondSelectedSlotInInventory;
        public const int ButtonBasedTextSize = 15;
        public const string ButtonBasedFontFamily = "Arial";
        public readonly Color BaseWorldColor = Color.FromArgb(119, 185, 129);
        public UserView()
        {
            menu = new Menu();
            menu.StartGame += () => StartGame();
            menu.CloseForm += () => Close();
            SizeChanged += (sender, e) => menu.ChangeButtonTextSize(ClientSize.Height * ButtonBasedTextSize / 300);
            Controls.Add(menu.MenuTable);
        }

        public void StartGame()
        {
            Controls.Clear();
            game = new GameModel(40);
            visual = new GameVisual(game);
            controls = new GameControls(game, visual);
            var keyBar = new TextBox() { Size = new Size(0,0)};
            controls.SetKeyCommands(keyBar);
            controls.ViewInventory += () => ViewInventory(game.Player.Inventory);
            controls.CloseInventory += () => CloseInventory();
            var tableView = visual.GetWorldVisual(game.Player.Position);
            var table = SetImages(SetGameTable(), tableView);
            gameView = table;
            visual.ChangeOneCellView += (row, column, worldCell, player) =>
            {
                var image = player == null ? Image.FromFile(game.FileName(worldCell)) : Image.FromFile(game.FileName(player));
                var pict = (PictureBox)table.GetControlFromPosition(row, column);
                pict.Image = image;
                pict.SizeMode = PictureBoxSizeMode.Zoom;
            };
            visual.ChangeInventoryCellView += (number, cell) =>
            {
                var image = Image.FromFile(game.FileName(cell));
                var pict = number == 1 ? firstSelectedSlotInInventory : secondSelectedSlotInInventory;
                pict.Image = image;
                pict.SizeMode = PictureBoxSizeMode.Zoom;
                pict.Tag = Color.Transparent;
                pict.Refresh();
                if (number == 1)
                    firstSelectedSlotInInventory = null;
                else
                    secondSelectedSlotInInventory = null;
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
            ((PictureBox)table.GetControlFromPosition(GameVisual.ViewFieldSize / 2, GameVisual.ViewFieldSize / 2)).Image = playerImage;
            return table;
        }

        public void ViewInventory(Inventory inventory)
        {
            var table = new TableLayoutPanel() {BackColor = Color.Gray, Dock = DockStyle.Fill };
            for (int i = 0; i < inventory.InventorySlots.GetLength(0); i++)
            {
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 2 / inventory.InventorySlots.GetLength(1)));
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / inventory.InventorySlots.GetLength(0)));
            }
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 2));
            for (int i = 0; i < inventory.InventorySlots.GetLength(0); i++)
                for (int j = 0; j < inventory.InventorySlots.GetLength(1); j++)
                {
                    var row = i;
                    var column = j;
                    var image = Image.FromFile(game.FileName(inventory.InventorySlots[i,j]));
                    var p = new PictureBox() {BackColor = Color.White,Image = image,Dock = DockStyle.Fill };
                    p.SizeMode = PictureBoxSizeMode.Zoom;
                    p.Tag = Color.Transparent;
                    p.Paint += (sender, e) =>
                    {
                        var penWidth = p.Width / 4;
                        e.Graphics.DrawRectangle(new Pen((Color)p.Tag, penWidth), 0, 0, p.Width - 2, p.Height - 2);
                    };
                    p.Click += (sender, e) =>
                    {
                        p.Tag = Color.Green;
                        p.Refresh();
                        if (game.Player.Inventory.AddSlot(row, column))
                        {
                            if (firstSelectedSlotInInventory == null)
                                firstSelectedSlotInInventory = p;
                            else
                                secondSelectedSlotInInventory = p;
                        }
                        else
                        {
                            firstSelectedSlotInInventory.Tag = Color.Transparent;
                            firstSelectedSlotInInventory.Refresh();
                            firstSelectedSlotInInventory = secondSelectedSlotInInventory;
                            secondSelectedSlotInInventory = p;
                        }
                    };
                    table.Controls.Add(p, j,i);
                }
            for (int i = 0; i<inventory.InventorySlots.GetLength(0); i++)
            {
                var p = new PictureBox() { BackColor = Color.Transparent,Dock = DockStyle.Fill };
                table.Controls.Add(p, inventory.InventorySlots.GetLength(1),i);
            }
            var currentSlot = (PictureBox)table.GetControlFromPosition(0, 0);
            currentSlot.BorderStyle = BorderStyle.FixedSingle;
            currentSlot.Paint += (sender, e) =>
            {
                var penWidth = currentSlot.Width / 6;
                e.Graphics.DrawRectangle(new Pen(Color.Black,penWidth), 0, 0, currentSlot.Width-2, currentSlot.Height-2);
            };
            Controls.Remove(gameView);
            Controls.Add(table);
            lastInventoryView = table;
        }

        public void CloseInventory()
        {
            firstSelectedSlotInInventory = null;
            secondSelectedSlotInInventory = null;
            game.Player.Inventory.ClearSlots();
            Controls.Remove(lastInventoryView);
            Controls.Add(gameView);
        }
    }
}