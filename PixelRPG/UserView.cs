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
        private PictureBox ArmSlot;
        private PictureBox firstSelectedSlotInInventory;
        private PictureBox secondSelectedSlotInInventory;
        private List<PictureBox> craftImages = new List<PictureBox>();
        public const float CurrentSlotPercentSize = 10f;
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
            var tableView = visual.GetWorldVisual(game.Player.Position);
            var table = SetImages(SetGameTable(), tableView);
            gameView = table;
            SetAllVisualDelegates(table);
            AddAllPlayerData();
            Controls.Add(table);
            Controls.Add(keyBar);
            keyBar.Focus();
            keyBar.Select();
        }

        public void AddAllPlayerData()
        {
            SetArmSlot();
            Controls.Add(ArmSlot);
        }

        public void SetArmSlot()
        {
            var currentSlot = new PictureBox()
            {
                BackColor = Color.Gray,
                Image = Image.FromFile(game.FileName(game.Player.Inventory.InventorySlots[0, 0])),
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size((int)(ClientSize.Width * 0.01 * CurrentSlotPercentSize), (int)(ClientSize.Height * 0.01 * CurrentSlotPercentSize)),
                Location = new Point((int)(ClientSize.Width * 0.01 * (100 - CurrentSlotPercentSize)), (int)(ClientSize.Height * 0.01 * (100 - CurrentSlotPercentSize)))
            };
            currentSlot.Paint += (sender, e) =>
            {
                var penWidth = currentSlot.Width / 20;
                e.Graphics.DrawRectangle(new Pen(Color.Black, penWidth), 0, 0, currentSlot.Width - 2, currentSlot.Height - 2);
            };
            ArmSlot = currentSlot;
            SizeChanged += (sender, e) =>
            {
                ArmSlot.Size = new Size((int)(ClientSize.Width * 0.01 * CurrentSlotPercentSize), (int)(ClientSize.Height * 0.01 * CurrentSlotPercentSize));
                ArmSlot.Location = new Point((int)(ClientSize.Width * 0.01 * (100 - CurrentSlotPercentSize)), (int)(ClientSize.Height * 0.01 * (100 - CurrentSlotPercentSize)));
            };
        }

        public void SetAllVisualDelegates(TableLayoutPanel table)
        {
            visual.OpenInventoryView += () => ViewInventory(game.Player.Inventory);
            visual.CloseInventoryView += () => CloseInventory();
            visual.OpenMenuView += () => OpenMenu();
            visual.ChangeCraftImagesView += (inv) => ChangeCraftsImages(inv);
            visual.ChangeOneCellView += (row, column, worldCell, player,mob) =>
            {
                Image image;
                if (player == null)
                {
                    if (mob == null)
                        image = Image.FromFile(game.FileName(worldCell));
                    else
                        image = Image.FromFile(game.FileName(mob));
                }
                else
                    image = Image.FromFile(game.FileName(player));
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
            visual.ChangeCurrentInventorySlotView += (el) =>
            {
                ArmSlot.Image = Image.FromFile(game.FileName(el));
            };
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

        public TableLayoutPanel SetImages(TableLayoutPanel table, (WorldElement[,] Elements, Dictionary<Point, Mob> Mobs) tableView)
        {
            for (int i = 0; i < GameVisual.ViewFieldSize; i++)
                for (int j = 0; j < GameVisual.ViewFieldSize; j++)
                {
                    Image image;
                    if (tableView.Mobs.ContainsKey(new Point(i, j)))
                        image = Image.FromFile(game.FileName(tableView.Mobs[new Point(i,j)]));
                    else
                        image = Image.FromFile(game.FileName(tableView.Elements[i, j]));
                    var p = new PictureBox() { Dock = DockStyle.Fill, Image = image };
                    p.SizeMode = PictureBoxSizeMode.Zoom;
                    table.Controls.Add(p, i, j);
                }
            var playerImage = Image.FromFile(game.FileName(game.Player));
            ((PictureBox)table.GetControlFromPosition(GameVisual.ViewFieldSize / 2, GameVisual.ViewFieldSize / 2)).Image = playerImage;
            return table;
        }

        public TableLayoutPanel SetInventoryTable(Inventory inventory)
        {
            var table = new TableLayoutPanel() { BackColor = Color.Gray, Dock = DockStyle.Fill };
            for (int i = 0; i < inventory.InventorySlots.GetLength(0); i++)
            {
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 2 / inventory.InventorySlots.GetLength(1)));
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / inventory.InventorySlots.GetLength(0)));
            }
            var rightSide = inventory.GetRightSide();
            var currentInventory = inventory as ArmCraft;
            if (currentInventory != null)
                rightSide = currentInventory.GetRightSide();
            for (int j = 0; j < rightSide.GetLength(0); j++)
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 2 / rightSide.GetLength(0)));
            return table;
        }

        public PictureBox GetInventoryImage(Image image,int row, int column,InventoryTypes type)
        {
            var p = new PictureBox() { BackColor = Color.White, Image = image, Dock = DockStyle.Fill };
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
                if (game.Player.Inventory.AddSlot(row, column, type))
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
            return p;
        }

        public void SetImagesInInventory(TableLayoutPanel table,Inventory inventory)
        {
            for (int i = 0; i < inventory.InventorySlots.GetLength(0); i++)
                for (int j = 0; j < inventory.InventorySlots.GetLength(1); j++)
                {
                    var image = Image.FromFile(game.FileName(inventory.InventorySlots[i, j]));
                    var p = GetInventoryImage(image, i, j,InventoryTypes.Main);
                    table.Controls.Add(p, j, i);
                }
            var rightSide = inventory.GetRightSide();
            var currentInventory = inventory as ArmCraft;
            if (currentInventory != null)
                rightSide = currentInventory.GetRightSide();
            for (int j = 0; j< rightSide.GetLength(1); j++)
                for (int i = 0;i < rightSide.GetLength(0);i++)
                {
                    var p = new PictureBox();
                    switch (rightSide[i,j]) 
                    {
                        case InventoryTypes.None:
                            p.BackColor = Color.Transparent;
                            break;
                        case InventoryTypes.Craft:
                            var image = Image.FromFile(game.FileName(currentInventory.CraftZone[j-1, i-1]));
                            p = GetInventoryImage(image, j - 1, i - 1, InventoryTypes.Craft);
                            craftImages.Add(p);
                            break;
                        case InventoryTypes.Result:
                            image = Image.FromFile(game.FileName(currentInventory
                                .CraftResult[j - 1 - currentInventory.CraftZone.GetLength(1) - 1,i-1]));
                            p = GetInventoryImage(image, j - 1 - currentInventory.CraftZone.GetLength(1) - 1, i - 1, InventoryTypes.Result);
                            craftImages.Add(p);
                            break;
                    }
                    table.Controls.Add(p, i+inventory.InventorySlots.GetLength(0),j);
                }
            HighlightFirstSlot(table);
        }

        public void HighlightFirstSlot(TableLayoutPanel table)
        {
            var currentSlot = (PictureBox)table.GetControlFromPosition(0, 0);
            currentSlot.BorderStyle = BorderStyle.FixedSingle;
            currentSlot.Paint += (sender, e) =>
            {
                var penWidth = currentSlot.Width / 6;
                e.Graphics.DrawRectangle(new Pen(Color.Black, penWidth), 0, 0, currentSlot.Width - 2, currentSlot.Height - 2);
            };
        }

        private void ChangeCraftsImages(Inventory inventory)
        {
            var currentInventory = inventory as ArmCraft;
            if (currentInventory != null)
            {
                var count = 0;
                for (int i = 0;i<currentInventory.CraftZone.GetLength(0);i++)
                    for (int j = 0;j<currentInventory.CraftZone.GetLength(1);j++)
                    {
                        craftImages[count].Image = Image.FromFile(game.FileName(currentInventory.CraftZone[i, j]));
                        count++;
                    }
                for (int i = 0;i< currentInventory.CraftResult.GetLength(0);i++)
                    for(int j = 0; j<currentInventory.CraftResult.GetLength(1);j++)
                    {
                        craftImages[count].Image = Image.FromFile(game.FileName(currentInventory.CraftResult[i, j]));
                        count++;
                    }
            }
        }

        public void ViewInventory(Inventory inventory)
        {
            var table = SetInventoryTable(inventory);
            SetImagesInInventory(table, inventory);
            Controls.Remove(gameView);
            Controls.Remove(ArmSlot);
            Controls.Add(table);
            lastInventoryView = table;
        }

        public void CloseInventory()
        {
            craftImages = new List<PictureBox>();
            firstSelectedSlotInInventory = null;
            secondSelectedSlotInInventory = null;
            game.Player.Inventory.ClearSlots();
            Controls.Remove(lastInventoryView);
            Controls.Add(ArmSlot);
            Controls.Add(gameView);
        }

        public void OpenMenu()
        {
            Controls.Clear();
            Controls.Add(menu.MenuTable);
        }
    }
}