using System;
using System.IO;
using System.Windows.Forms;

namespace PixelRPG
{
    public class Menu
    {
        public readonly int ButtonCount;
        public const float SideIndentPercent = 20f;
        private Button[] buttons;
        public readonly TableLayoutPanel MenuTable;
        private int currentButtonIndex=0;
        public TextBox KeyBar { get; private set; }
        public Menu()
        {
            CreateButtons();
            ButtonCount = buttons.Length;
            MenuTable = GetMenu();
        }

        public event Action StartGame;
        public event Action CloseForm;

        public void ChangeButtonTextSize(int size)
        {
            if (size > 0)
                foreach (var button in buttons)
                    button.Font = new Font(UserView.ButtonBasedFontFamily, size);
            else if (size < 0)
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

        private void CreateButtons()
        {
            buttons = new Button[3];
            buttons[0] = CreateMenuButton("Новая игра", (sender, e) =>  StartGame() );
            buttons[1] = CreateMenuButton("Настройки", (sender, e) => { throw new Exception("Не Сделано"); });
            buttons[2] = CreateMenuButton("Выход", (sender, e) => CloseForm() );
        }

        public Button CreateMenuButton(string text, EventHandler act)
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
            button.Click += act;
            return button;
        }
    }
}
