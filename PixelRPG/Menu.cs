using System;
using System.Windows.Forms;

namespace PixelRPG
{
    public partial class UserView
    {
        public class Menu
        {
            const int ButtonCount = 2;
            const float SideIndentPercent = 15f;
            private Button[] buttons;
            public readonly TableLayoutPanel MenuTable;
            public Menu()
            {
                MenuTable = GetMenu();
            }

            public event Action<TableLayoutPanel> StartGame;

            public void ChangeButtonTextSize(int size)
            {
                if (size >= 0)
                    foreach (var button in buttons)
                        button.Font = new Font(UserView.ButtonBasedFontFamily, size);
                else
                    throw new ArgumentException();
            }

            public TableLayoutPanel GetMenu()
            {
                var table = new TableLayoutPanel() { Dock = DockStyle.Fill, BackColor = Color.Purple };
                SetTable(table);
                SetButtons(table);
                return table;
            }

            public void SetTable(TableLayoutPanel table)
            {
                for (int i = 0; i < ButtonCount + 2; i++)
                {
                    table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, SideIndentPercent));
                    table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 - 2 * SideIndentPercent));
                    table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, SideIndentPercent));
                    table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / (ButtonCount + 2)));
                }
            }

            public void SetButtons(TableLayoutPanel table)
            {
                GetButtons();
                for (int i = 0; i < ButtonCount + 2; i++)
                    for (int j = 0; j < 3; j++)
                        if (j == 1 && i != 0 && i != ButtonCount + 1)
                            table.Controls.Add(buttons[i - 1], j, i);
                        else
                        {
                            var p = new PictureBox() { Dock = DockStyle.Fill, BackColor = Color.Transparent };
                            table.Controls.Add(p, j, i);
                        }
            }

            private void GetButtons()
            {
                buttons = new Button[ButtonCount];
                buttons[0] = CreateMenuButton("Новая игра");
                buttons[0].Click += (sender, e) => { StartGame(MenuTable); };
                buttons[1] = CreateMenuButton("Настройки");
                buttons[1].Click += (sender, e) => { throw new Exception("Не Сделано"); };
                if (buttons[ButtonCount - 1] == null)
                    throw new Exception("Количество кнопок не совпадает");
            }

            public Button CreateMenuButton(string text)
            {
                var button = new Button()
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Black,
                    Text = text,
                    ForeColor = Color.White,
                    Font = new Font(UserView.ButtonBasedFontFamily, UserView.ButtonBasedTextSize)
                };
                button.FlatAppearance.BorderSize = 0;
                button.FlatStyle = FlatStyle.Flat;
                return button;
            }
        }
    }
}
