using System;
using System.Security.Policy;
using PixelRPG;

public class GameControls
{
    private GameModel game;
    private GameVisual visual;
    private static Dictionary<char, Sides> turns = new Dictionary<char, Sides>()
    {
        { 'A',Sides.Left },
        {'S',Sides.Down },
        {'D',Sides.Right },
        {'W',Sides.Up }
    };
    private static Dictionary<Sides,Point> moves = new Dictionary<Sides,Point>()
    {
        {Sides.Left,new Point(-1,0) },
        {Sides.Down, new Point(0,1) },
        {Sides.Right, new Point(1,0) },
        {Sides.Up, new Point(0,-1)}
    };
    private bool IsInventoryOpen = false;
    public GameControls( GameModel game, GameVisual visual)
	{
        this.game = game;
        this.visual = visual;
	}

    public event Action OpenInventory;
    public event Action CloseInventory;
    public event Action OpenMenu;
    public event Action<Inventory> ChangeCraftImages;

    public void Move(Point newPosition)=>
        visual.GetWorldVisual(newPosition);


    public void SetKeyCommands(TextBox keyBar)
    {
        keyBar.KeyPress += (sender, e) =>
        {
            if (e.KeyChar == (char)Keys.Escape)
                OpenMenu();
        };
        keyBar.KeyPress += (sender, e) =>
        {
            var newPosition = new Point(-1,-1);
            var newView = game.Player.Direction;
            var controlChar = char.ToUpper(e.KeyChar);
            if (turns.ContainsKey(controlChar))
            {
                newView = turns[controlChar];
                var point = moves[newView];
                newPosition = new Point(game.Player.Position.X+point.X, game.Player.Position.Y+point.Y);
            }
            if (newView != game.Player.Direction)
            {
                game.SetPlayerView(newView);
                visual.ChangeOneCell(GameVisual.ViewFieldSize / 2, GameVisual.ViewFieldSize / 2,null, new Player(game.Player.Type,game.Player.Position,newView));
            }
            else if (game.InBounds(newPosition) && game.IsStepablePoint(newPosition))
            {
                Move(newPosition);
                game.SetPlayerPosition(newPosition);
                if (game.PickItem(newPosition))
                    visual.ChangeCurrentInventorySlot(game.Player.Inventory.InventorySlots[0, 0]);
            }
        };
        keyBar.KeyPress += (sender, e) => 
        {
            var frontPoint = new Point(game.Player.Position.X + moves[game.Player.Direction].X, game.Player.Position.Y + moves[game.Player.Direction].Y);
            var frontElement = game.InBounds(frontPoint) ? game.World[frontPoint.X, frontPoint.Y] : game.OutOfBounds;
            var controlChar = char.ToUpper(e.KeyChar);
            if (controlChar ==(char)Keys.L)
                if (frontElement.BreakLevel <= game.Player.Inventory.InventorySlots[0,0].PowerToBreakOtherEl)
                {
                    game.World[frontPoint.X, frontPoint.Y] = frontElement.Drop;
                    visual.ChangeOneCellByWorldCoords(frontPoint.X, frontPoint.Y,frontElement.Drop);
                }
            if (controlChar == (char)Keys.P)
                if (frontElement.Name == "Empty" && game.Player.Inventory.InventorySlots[0, 0].Name != "Empty")
                {
                    game.World[frontPoint.X, frontPoint.Y] = game.Player.Inventory.InventorySlots[0, 0];
                    game.Player.Inventory.InventorySlots[0, 0] = game.WorldElementsList[0];
                    visual.ChangeOneCellByWorldCoords(frontPoint.X, frontPoint.Y, game.World[frontPoint.X, frontPoint.Y]);
                    visual.ChangeCurrentInventorySlot(game.Player.Inventory.InventorySlots[0, 0]);
                }

        };
        keyBar.KeyPress += (sender, e) =>
        {
            var controlChar = char.ToUpper(e.KeyChar);
            if (controlChar == (char)Keys.I)
                if (IsInventoryOpen)
                {
                    CloseInventory();
                    IsInventoryOpen = false;
                }
                else
                {
                    OpenInventory();
                    IsInventoryOpen = true;
                }
            if (controlChar == (char)Keys.J)
            {
                var data = game.Player.Inventory.ChangeSelectedSlots();
                if (data.IsComplete)
                    switch (data.First.Type)
                    {
                        case InventoryTypes.Main:
                            ChangeInventoryVisualSwitchSecondType(game.Player.Inventory.InventorySlots, data.Second.Type, data); 
                            break;
                        case InventoryTypes.Craft:
                            ChangeInventoryVisualSwitchSecondType(game.Player.Inventory.CraftZone, data.Second.Type, data);
                            break;
                        case InventoryTypes.Result:
                            ChangeInventoryVisualSwitchSecondType(game.Player.Inventory.CraftResult, data.Second.Type, data);
                            break;
                    }
                visual.ChangeCurrentInventorySlot(game.Player.Inventory.InventorySlots[0, 0]);
            }
            if (controlChar == (char)Keys.E)
            {
                if (!IsInventoryOpen && game.Player.Inventory.SetItemInFirstSlot())
                    visual.ChangeCurrentInventorySlot(game.Player.Inventory.InventorySlots[0, 0]);
            }
            if (controlChar == (char)Keys.C)
            {
                if (IsInventoryOpen && game.Player.Inventory.Craft(game.Crafts2by2))
                {
                    ChangeCraftImages(game.Player.Inventory);
                }
            }
        };
    }

    public void ChangeInventoryVisualSwitchSecondType(WorldElement[,] first, InventoryTypes second, 
        (bool IsComplete, (int X, int Y, InventoryTypes Type) First, (int X, int Y, InventoryTypes Type) Second) data)
    {
        switch(second)
        {
            case InventoryTypes.Main:
                visual.ChangeInventoryCell(1, first[data.First.X, data.First.Y]);
                visual.ChangeInventoryCell(2, game.Player.Inventory.InventorySlots[data.Second.X, data.Second.Y]);
                break;
            case InventoryTypes.Craft:
                visual.ChangeInventoryCell(1, first[data.First.X, data.First.Y]);
                visual.ChangeInventoryCell(2, game.Player.Inventory.CraftZone[data.Second.X, data.Second.Y]);
                break;
            case InventoryTypes.Result:
                visual.ChangeInventoryCell(1, first[data.First.X, data.First.Y]);
                visual.ChangeInventoryCell(2, game.Player.Inventory.CraftResult[data.Second.X, data.Second.Y]);
                break;
        }
    }
}
