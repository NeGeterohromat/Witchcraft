using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelRPG
{
    public class Settings
    {
        public TableLayoutPanel SettingsTable {  get; private set; }
        private int SettingsCount = 4;
        public Settings() 
        {
            SettingsTable = GetSettingsTable();
        }

        public static event Action<int> WorldSizeChanged;
        public static event Action<int> SoundVolumeChanged;
        public static event Action<bool> EnemyDifficultChanged;
        public static event Action<int> VisualSizeChanged;
        public static event Action BackToMenu;

        private TableLayoutPanel GetSettingsTable()
        {
            var table = new TableLayoutPanel() { Dock = DockStyle.Fill, BackColor = Color.LightGreen };
            SetTable(table);
            SetControls(table);
            return table;
        }

        public void SetTable(TableLayoutPanel table)
        {
            for (int i = 0; i < SettingsCount + 3; i++)
            {
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, Menu.SideIndentPercent));
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, (100 - 2 * Menu.SideIndentPercent) / 2));
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, (100 - 2 * Menu.SideIndentPercent) / 2));
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, Menu.SideIndentPercent));
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / (SettingsCount + 2)));
            }
        }

        public void SetControls(TableLayoutPanel table)
        {
            var transparent = new PictureBox() { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            var worldSizeText = CreateSettingsTextBox(UserView.gameSize, WorldSizeChanged);
            var soundText = CreateSettingsTextBox(UserView.BaseSoundVolume, SoundVolumeChanged);
            var visualSizeText = CreateSettingsTextBox(GameVisual.ViewFieldSize, VisualSizeChanged);
            var easyButton = Menu.CreateMenuButton("Без враждебных мобов", (sender, e) => EnemyDifficultChanged(false));
            var hardButton = Menu.CreateMenuButton("С враждебными мобами", (sender, e) => EnemyDifficultChanged(true));
            var exitButton = Menu.CreateMenuButton("Вернуться в меню", (sender, e) => BackToMenu());
            var difficultyTable = new TableLayoutPanel() { Dock = DockStyle.Fill };
            difficultyTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            difficultyTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            difficultyTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            difficultyTable.Controls.Add(easyButton,0,0);
            difficultyTable.Controls.Add(hardButton,1,0);
            SetControlsInOneRow(table, 0, transparent, transparent, transparent, transparent);
            SetControlsInOneRow(table, 1, transparent, new Label() { Dock = DockStyle.Fill, Text = "Размер мира", Font = new Font(UserView.ButtonBasedFontFamily, UserView.ButtonBasedTextSize) },
                worldSizeText, transparent);
            SetControlsInOneRow(table, 2, transparent, new Label() { Dock = DockStyle.Fill, Text = "Музыка и звуки", Font = new Font(UserView.ButtonBasedFontFamily, UserView.ButtonBasedTextSize) },
                soundText,transparent);
            SetControlsInOneRow(table, 3, transparent, new Label() { Dock = DockStyle.Fill, Text = "Сложность игры", Font = new Font(UserView.ButtonBasedFontFamily, UserView.ButtonBasedTextSize) },
                difficultyTable, transparent);
            SetControlsInOneRow(table, 4, transparent, new Label() { Dock = DockStyle.Fill, Text = "Размер области отображения", Font = new Font(UserView.ButtonBasedFontFamily, UserView.ButtonBasedTextSize) },
                visualSizeText, transparent);
            SetControlsInOneRow(table, 5, transparent, exitButton, transparent, transparent);
            SetControlsInOneRow(table, 6, transparent, transparent, transparent, transparent);
        }

        public void SetControlsInOneRow(TableLayoutPanel table, int row, Control first, Control second, Control third, Control fourth)
        {
            table.Controls.Add(first, 0, row);
            table.Controls.Add(second, 1, row);
            table.Controls.Add(third, 2, row);
            table.Controls.Add(fourth, 3, row);
        }

        public TextBox CreateSettingsTextBox(int baseValue, Action<int> act)
        {
            var text = new TextBox() { Dock = DockStyle.Fill, Font = new Font(UserView.ButtonBasedFontFamily, UserView.ButtonBasedTextSize) };
            text.KeyPress += (sender, e) =>
            {
                var size = baseValue;
                if (e.KeyChar == (char)Keys.Enter)
                {
                    try { size = int.Parse(text.Text); } catch { }
                    act(size);
                }
            };
            return text;
        }
    }
}
