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
        private Menu mainMenu;
        private Menu escapeMenu;
        private TableLayoutPanel lastInventoryView;
        private TableLayoutPanel gameView;
        private PictureBox ArmSlot;
        private PictureBox PlayerHealth;
        private PictureBox PlayerFood;
        private PictureBox PlayerMana;
        private PictureBox firstSelectedSlotInInventory;
        private PictureBox secondSelectedSlotInInventory;
        private List<PictureBox> craftImages = new List<PictureBox>();
        public const float CurrentSlotPercentSize = 10f;
        public const int ButtonBasedTextSize = 15;
        public const string ButtonBasedFontFamily = "Arial";
        public readonly Color BaseWorldColor = Color.FromArgb(119, 185, 129);
        public UserView()
        {
            mainMenu = new Menu(Color.Purple,
                ("Новая игра", (sender, e) => StartGame()),
                ("Настройки", (sender, e) =>  throw new Exception("Не Сделано")),
                ("Выход", (sender, e) => Close())
                );
            escapeMenu = new Menu(Color.Orange,
                ("В главное меню",(sender,e)=> OpenMenu(MenuType.Main)),
                ("Возродиться",(sender,e)=>RestartGame())
                );
            SizeChanged += (sender, e) => mainMenu.ChangeButtonTextSize(ClientSize.Height * ButtonBasedTextSize / 300);
            SizeChanged += (sender, e) => escapeMenu.ChangeButtonTextSize(ClientSize.Height * ButtonBasedTextSize / 300);
            Controls.Add(mainMenu.MenuTable);
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
            controls.mobMoveTimer.Start();
            GC.Collect();
        }

        public void RestartGame()
        {
            Controls.Clear();
            game.Player.IncreaseHealth(20);
            game.Player.IncreaseSatiety(20);
            game.Player.IncreaseMana(20);
            var keyBar = new TextBox() { Size = new Size(0, 0) };
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
            controls.mobMoveTimer.Start();
            GC.Collect();
        }

        public void AddAllPlayerData()
        {
            SetArmSlot();
            SetPlayerHealth();
            SetPlayerFood();
            SetPlayerMana();
            AddPlayerDataView();
        }

        public PictureBox SetOnePlayerData(Image image, Size size, Point location)
        {
            var element = new PictureBox()
            {
                BackColor = Color.Gray,
                Image = image,
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = size,
                Location = location
            };
            element.Paint += (sender, e) =>
            {
                var penWidth = element.Width / 20;
                e.Graphics.DrawRectangle(new Pen(Color.Black, penWidth), 0, 0, element.Width - 2, element.Height - 2);
            };
            return element;
        }

        public void SetPlayerMana()
        {
            PlayerMana = SetOnePlayerData(Image.FromFile(@"images/icons/ManaFull.png"),
                new Size((int)(ClientSize.Width * 0.01 * CurrentSlotPercentSize / 2), (int)(ClientSize.Height * 0.01 * CurrentSlotPercentSize)),
                new Point((int)(ClientSize.Width * 0.01 * (100 - CurrentSlotPercentSize / 2)), (int)(ClientSize.Height * 0.01 * (100 - CurrentSlotPercentSize * 3))));
            SizeChanged += (sender, e) =>
            {
                PlayerMana.Size = new Size((int)(ClientSize.Width * 0.01 * CurrentSlotPercentSize / 2), (int)(ClientSize.Height * 0.01 * CurrentSlotPercentSize));
                PlayerMana.Location = new Point((int)(ClientSize.Width * 0.01 * (100 - CurrentSlotPercentSize / 2)), (int)(ClientSize.Height * 0.01 * (100 - CurrentSlotPercentSize * 3)));
            };
        }

        public void SetPlayerFood()
        {
            PlayerFood = SetOnePlayerData(Image.FromFile(@"images/icons/FoodFull.png"),
                new Size((int)(ClientSize.Width * 0.01 * CurrentSlotPercentSize / 2), (int)(ClientSize.Height * 0.01 * CurrentSlotPercentSize)),
                new Point((int)(ClientSize.Width * 0.01 * (100 - CurrentSlotPercentSize)), (int)(ClientSize.Height * 0.01 * (100 - CurrentSlotPercentSize * 2))));
            SizeChanged += (sender, e) =>
            {
                PlayerFood.Size = new Size((int)(ClientSize.Width * 0.01 * CurrentSlotPercentSize / 2), (int)(ClientSize.Height * 0.01 * CurrentSlotPercentSize));
                PlayerFood.Location = new Point((int)(ClientSize.Width * 0.01 * (100 - CurrentSlotPercentSize)), (int)(ClientSize.Height * 0.01 * (100 - CurrentSlotPercentSize * 2)));
            };
        }

        public void SetPlayerHealth()
        { 
            PlayerHealth=SetOnePlayerData(Image.FromFile(@"images/icons/HeartFull.png"),
                 new Size((int)(ClientSize.Width * 0.01 * CurrentSlotPercentSize / 2), (int)(ClientSize.Height * 0.01 * CurrentSlotPercentSize)),
                 new Point((int)(ClientSize.Width * 0.01 * (100 - CurrentSlotPercentSize / 2)), (int)(ClientSize.Height * 0.01 * (100 - CurrentSlotPercentSize * 2))));
            SizeChanged += (sender, e) => 
            {
                PlayerHealth.Size = new Size((int)(ClientSize.Width * 0.01 * CurrentSlotPercentSize / 2), (int)(ClientSize.Height * 0.01 * CurrentSlotPercentSize));
                PlayerHealth.Location = new Point((int)(ClientSize.Width * 0.01 * (100 - CurrentSlotPercentSize / 2)), (int)(ClientSize.Height * 0.01 * (100 - CurrentSlotPercentSize * 2)));
            };
        }

        public void SetArmSlot()
        {
            ArmSlot = SetOnePlayerData(Image.FromFile(game.FileName(game.Player.Inventory.InventorySlots[0, 0])),
                new Size((int)(ClientSize.Width * 0.01 * CurrentSlotPercentSize), (int)(ClientSize.Height * 0.01 * CurrentSlotPercentSize)),
                new Point((int)(ClientSize.Width * 0.01 * (100 - CurrentSlotPercentSize)), (int)(ClientSize.Height * 0.01 * (100 - CurrentSlotPercentSize))));
            SizeChanged += (sender, e) =>
            {
                ArmSlot.Size = new Size((int)(ClientSize.Width * 0.01 * CurrentSlotPercentSize), (int)(ClientSize.Height * 0.01 * CurrentSlotPercentSize));
                ArmSlot.Location = new Point((int)(ClientSize.Width * 0.01 * (100 - CurrentSlotPercentSize)), (int)(ClientSize.Height * 0.01 * (100 - CurrentSlotPercentSize)));
            };
        }
        public Bitmap ChangeImageAlpha(Image image, int a)
        {
            var bitmap = (Bitmap)image;
            Bitmap bmp = new Bitmap(bitmap.Width, bitmap.Height);
            for (int i = 0; i < bmp.Width; i++)
                for (int j = 0; j < bmp.Height; j++)
                    if (bitmap.GetPixel(i, j).A != 0)
                        bmp.SetPixel(i, j, Color.FromArgb(a, bitmap.GetPixel(i, j).R, bitmap.GetPixel(i, j).G, bitmap.GetPixel(i, j).B));
            return bmp;
        }

        public Bitmap CompareImages(Image image1, Image image2,int percentSizeSecondImage)
        {
            var bmpUnder = (Bitmap)image1;
            var bmpOn = (Bitmap)image2;
            var g = Graphics.FromImage(bmpUnder);
            g.DrawImage(bmpOn,
                (bmpUnder.Size.Width - bmpUnder.Size.Width * percentSizeSecondImage / 100) / 2,
                (bmpUnder.Size.Height - bmpUnder.Size.Height * percentSizeSecondImage / 100) / 2,
                bmpUnder.Size.Width * percentSizeSecondImage / 100,
                bmpUnder.Size.Height * percentSizeSecondImage / 100);
            g.Dispose();
            return bmpUnder;
        }

        public void SetAllVisualDelegates(TableLayoutPanel table)
        {
            visual.ChangeManaView += () =>
            {
                var imageUnder = Image.FromFile(@"images/icons/ManaEmpty.png");
                var imageOn = ChangeImageAlpha(Image.FromFile(@"images/icons/ManaFull.png"), 255 * game.Player.Mana / game.Player.MaxMana);
                PlayerMana.Image = CompareImages(imageUnder, imageOn, 100);
            };
            visual.ChangeFoodView += () =>
            {
                var imageUnder = Image.FromFile(@"images/icons/FoodEmpty.png");
                var imageOn = ChangeImageAlpha(Image.FromFile(@"images/icons/FoodFull.png"), 255 * game.Player.Satiety / game.Player.MaxSatiety);
                PlayerFood.Image = CompareImages(imageUnder, imageOn, 100);
            };
            visual.ChangeHealthView += () =>
            {
                var imageUnder = Image.FromFile(@"images/icons/HeartEmpty.png");
                var imageOn = ChangeImageAlpha(Image.FromFile(@"images/icons/HeartFull.png"), 255 * game.Player.Health / game.Player.MaxHealth);
                PlayerHealth.Image = CompareImages(imageUnder, imageOn, 100);
            };
            visual.AddImageInFirstLay += (i, j, im) =>
            {
                var p = (PictureBox)table.GetControlFromPosition(i, j);
                p.Image = CompareImages(p.Image,im,70);
            };
            visual.OpenInventoryView += (inv) => ViewInventory(inv);
            visual.CloseInventoryView += (inv) => CloseInventory(inv);
            visual.OpenMenuView += (type) => OpenMenu(type);
            visual.ChangeCraftImagesView += (inv) => ChangeCraftsImages(inv);
            visual.ChangeOneCellView += (row, column, image,mode) =>
            {
                var pict = (PictureBox)table.GetControlFromPosition(row, column);
                if (pict.Margin.Equals(new Padding(0)) && mode != PictureBoxSizeMode.StretchImage)
                    pict.Margin = new Padding(3);
                if (mode == PictureBoxSizeMode.StretchImage)
                    pict.Margin = new Padding(0);
                pict.Image = image;
                pict.SizeMode = mode;
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

        public TableLayoutPanel SetImages(TableLayoutPanel table, (WorldElement[,] Elements, Dictionary<Point, Entity> Mobs) tableView)
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
            var currentPlayerInventory = inventory as ArmCraft;
            var currentChestInventory = inventory as Chest;
            if (currentPlayerInventory != null)
                rightSide = currentPlayerInventory.GetRightSide();
            if(currentChestInventory != null)
                rightSide = currentChestInventory.GetRightSide();
            for (int j = 0; j < rightSide.GetLength(0); j++)
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 2 / rightSide.GetLength(0)));
            return table;
        }

        public PictureBox GetInventoryImage(Image image,int row, int column,InventoryTypes type,Inventory inventory)
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
                if (inventory.AddSlot(row, column, type))
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
                    var p = GetInventoryImage(image, i, j,InventoryTypes.Main,inventory);
                    table.Controls.Add(p, j, i);
                }
            var rightSide = inventory.GetRightSide();
            var currentPlayerInventory = inventory as ArmCraft;
            var currentChestInventory = inventory as Chest;
            if (currentPlayerInventory != null)
                rightSide = currentPlayerInventory.GetRightSide();
            if (currentChestInventory != null)
                rightSide = currentChestInventory.GetRightSide();
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
                            var image = Image.FromFile(game.FileName(currentPlayerInventory.CraftZone[j-1, i-1]));
                            p = GetInventoryImage(image, j - 1, i - 1, InventoryTypes.Craft,inventory);
                            craftImages.Add(p);
                            break;
                        case InventoryTypes.Result:
                            image = Image.FromFile(game.FileName(currentPlayerInventory
                                .CraftResult[j - 1 - currentPlayerInventory.CraftZone.GetLength(1) - 1,i-1]));
                            p = GetInventoryImage(image, j - 1 - currentPlayerInventory.CraftZone.GetLength(1) - 1, i - 1, InventoryTypes.Result, inventory);
                            craftImages.Add(p);
                            break;
                        case InventoryTypes.Chest:
                            image = Image.FromFile(game.FileName(currentChestInventory.ChestInventory.InventorySlots[j,i-1]));
                            p = GetInventoryImage(image,j,i-1,InventoryTypes.Chest, inventory);
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

        public void RemovePlayerDataFromView()
        {
            Controls.Remove(ArmSlot);
            Controls.Remove(PlayerHealth);
            Controls.Remove(PlayerFood);
            Controls.Remove(PlayerMana);
        }

        public void AddPlayerDataView()
        {
            Controls.Add(ArmSlot);
            Controls.Add(PlayerHealth);
            Controls.Add(PlayerFood);
            Controls.Add(PlayerMana);
        }

        public void ViewInventory(Inventory inventory)
        {
            var table = SetInventoryTable(inventory);
            SetImagesInInventory(table, inventory);
            Controls.Remove(gameView);
            RemovePlayerDataFromView();
            Controls.Add(table);
            lastInventoryView = table;
        }

        public void CloseInventory(Inventory inventory)
        {
            craftImages = new List<PictureBox>();
            firstSelectedSlotInInventory = null;
            secondSelectedSlotInInventory = null;
            inventory.ClearSlots();
            Controls.Remove(lastInventoryView);
            AddPlayerDataView();
            Controls.Add(gameView);
        }

        public void OpenMenu(MenuType type)
        {
            Controls.Remove(gameView);
            Controls.Remove(lastInventoryView);
            Controls.Remove(escapeMenu.MenuTable);
            Controls.Remove(mainMenu.MenuTable);
            RemovePlayerDataFromView();
            switch (type)
            {
                case MenuType.Main:
                    Controls.Add(mainMenu.MenuTable);
                    break;
                case MenuType.Escape:
                    Controls.Add(escapeMenu.MenuTable);
                    break;
            }
        }
    }
}