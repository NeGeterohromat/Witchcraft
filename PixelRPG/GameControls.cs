using System;
using System.Security.Policy;
using PixelRPG;

public class GameControls
{
    private GameModel game;
    private GameVisual visual;
    public readonly System.Windows.Forms.Timer peacefulMobMoveTimer;
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
        peacefulMobMoveTimer = GetPeacefulMobMoveTimer();
	}

    public System.Windows.Forms.Timer GetPeacefulMobMoveTimer()
    {
        var timer = new System.Windows.Forms.Timer();
        timer.Interval = GameModel.peacefulMobMoveTick;
        timer.Tick += (sender, e) =>
        {
            foreach (var entity in visual.CurrentViewedMobs.Where(en=>en.Value.Action == EntityActionType.Peaceful))
                RandomEntityMove(entity.Value);
            visual.GetWorldVisual(game.Player.Position);
        };
        return timer;
    }

    private void RandomEntityMove(Entity entity)
    {
        var random = new Random();
        if (random.NextDouble() < GameModel.peacefulMobMoveChance)
        {
            if (random.NextDouble() > 1d / 2)
                entity.SetDirection((Sides)random.Next(0, 4));
            else
            {
                var point = moves[entity.Direction];
                var newPosition = new Point(entity.Position.X + point.X, entity.Position.Y + point.Y);
                if (game.InBounds(newPosition) && game.IsStepablePoint(newPosition))
                    game.MoveEntity(entity, newPosition);
            }
        }
    }

    public void Move(Point newPosition)=>
        visual.GetWorldVisual(newPosition);


    public void SetKeyCommands(TextBox keyBar)
    {
        keyBar.KeyPress += (sender, e) =>
        {
            var controlChar = char.ToUpper(e.KeyChar);
            EscapeIfChar(controlChar);
            MoveIfChar(controlChar);
            WorldInteractionIfChar(controlChar);
            ChangeInventoryIfChar(controlChar);
        };
    }

    public void ChangeInventoryVisualSwitchSecondType(WorldElement[,] first, InventoryTypes second, 
        (bool IsComplete, (int X, int Y, InventoryTypes Type) First, (int X, int Y, InventoryTypes Type) Second) data)
    {
        var armCraft = game.Player.Inventory as ArmCraft;
        var chest = game.Chests[new Point(game.Player.Position.X + moves[game.Player.Direction].X, game.Player.Position.Y + moves[game.Player.Direction].Y)];
        switch (second)
        {
            case InventoryTypes.Main:
                visual.ChangeInventoryCell(1, first[data.First.X, data.First.Y]);
                visual.ChangeInventoryCell(2, armCraft.InventorySlots[data.Second.X, data.Second.Y]);
                break;
            case InventoryTypes.Craft:
                visual.ChangeInventoryCell(1, first[data.First.X, data.First.Y]);
                visual.ChangeInventoryCell(2, armCraft.CraftZone[data.Second.X, data.Second.Y]);
                break;
            case InventoryTypes.Result:
                visual.ChangeInventoryCell(1, first[data.First.X, data.First.Y]);
                visual.ChangeInventoryCell(2, armCraft.CraftResult[data.Second.X, data.Second.Y]);
                break;
            case InventoryTypes.Chest:
                visual.ChangeInventoryCell(1, first[data.First.X, data.First.Y]);
                visual.ChangeInventoryCell(2, chest.ChestInventory.InventorySlots[data.Second.X, data.Second.Y]);
                break;
        }
    }

    private void EscapeIfChar(char controlChar)
    {
        if (controlChar == (char)Keys.Escape)
            visual.OpenMenu();
    }

    private void MoveIfChar(char controlChar)
    {
        var newPosition = new Point(-1, -1);
        var newView = game.Player.Direction;
        if (turns.ContainsKey(controlChar))
        {
            newView = turns[controlChar];
            var point = moves[newView];
            newPosition = new Point(game.Player.Position.X + point.X, game.Player.Position.Y + point.Y);
        }
        if (newView != game.Player.Direction)
        {
            game.SetPlayerView(newView);
            visual.ChangeOneCell(GameVisual.ViewFieldSize / 2, GameVisual.ViewFieldSize / 2, game.Player);
        }
        else if (game.InBounds(newPosition) && game.IsStepablePoint(newPosition))
        {
            game.MoveEntity(game.Player,newPosition);
            Move(newPosition);
            if (game.PickItem(game.Player.Inventory,newPosition))
                visual.ChangeCurrentInventorySlot(game.Player.Inventory.InventorySlots[0, 0]);
        }
    }

    private void WorldInteractionIfChar(char controlChar)
    {
        var frontPoint = new Point(game.Player.Position.X + moves[game.Player.Direction].X, game.Player.Position.Y + moves[game.Player.Direction].Y);
        var frontElement = game.InBounds(frontPoint) ? game.World[frontPoint.X, frontPoint.Y] : game.OutOfBounds;
        if (controlChar == (char)Keys.L)
        {
            if (frontElement.BreakLevel <= game.Player.Inventory.InventorySlots[0, 0].PowerToBreakOtherEl)
            {
                game.World[frontPoint.X, frontPoint.Y] = frontElement.Drop;
                visual.ChangeOneCellByWorldCoords(frontPoint.X, frontPoint.Y, frontElement.Drop);
            }
            else if (game.Mobs.ContainsKey(frontPoint))
            {
                var entity = game.Mobs[frontPoint];
                entity.DamageEntity(game.Player.Inventory.InventorySlots[0, 0].Damage);
                visual.ViewDamageEffect(entity.Position);
                if (entity.Health == 0)
                {
                    game.Mobs.Remove(frontPoint);
                    game.Chests[frontPoint] = new Chest(entity.Inventory,game.Player.Inventory);
                    game.World[entity.Position.X, entity.Position.Y] = game.AllWorldElements["Heap"];
                    visual.ChangeOneCellByWorldCoords(entity.Position.X, entity.Position.Y, game.World[entity.Position.X, entity.Position.Y]);
                }
            }
        }
        if (controlChar == (char)Keys.P)
        {
            if (frontElement.Name == "Empty" && !game.Mobs.ContainsKey(frontPoint) && game.Player.Inventory.InventorySlots[0, 0].Name != "Empty")
            {
                game.World[frontPoint.X, frontPoint.Y] = game.Player.Inventory.InventorySlots[0, 0];
                game.Player.Inventory.InventorySlots[0, 0] = game.NatureWorldElementsList[0];
                visual.ChangeOneCellByWorldCoords(frontPoint.X, frontPoint.Y, game.World[frontPoint.X, frontPoint.Y]);
                visual.ChangeCurrentInventorySlot(game.Player.Inventory.InventorySlots[0, 0]);
            }
            else if (!IsInventoryOpen && game.Chests.ContainsKey(frontPoint))
            {
                visual.OpenInventory(game.Chests[frontPoint]);
                IsInventoryOpen = true;
            }
        }
    }

    private void ChangeInventoryIfChar(char controlChar)
    {
        var frontPoint = new Point(game.Player.Position.X + moves[game.Player.Direction].X, game.Player.Position.Y + moves[game.Player.Direction].Y);
        if (controlChar == (char)Keys.I)
            if (IsInventoryOpen)
            {
                visual.CloseInventory();
                IsInventoryOpen = false;
                if (game.Chests.ContainsKey(frontPoint) && game.Chests[frontPoint].IsEmpty())
                {
                    game.World[frontPoint.X, frontPoint.Y] = game.AllWorldElements["Empty"];
                    visual.ChangeOneCellByWorldCoords(frontPoint.X, frontPoint.Y, game.World[frontPoint.X, frontPoint.Y]);
                }
            }
            else
            {
                visual.OpenInventory(game.Player.Inventory);
                IsInventoryOpen = true;
            }
        if (controlChar == (char)Keys.J)
        {
            var armCraft = game.Player.Inventory as ArmCraft;
            var chest = game.Chests[frontPoint];
            var data = chest == null? armCraft.ChangeSelectedSlots():chest.ChangeSelectedSlots();
            if (data.IsComplete)
                switch (data.First.Type)
                {
                    case InventoryTypes.Main:
                        ChangeInventoryVisualSwitchSecondType(armCraft.InventorySlots, data.Second.Type, data);
                        break;
                    case InventoryTypes.Craft:
                        ChangeInventoryVisualSwitchSecondType(armCraft.CraftZone, data.Second.Type, data);
                        break;
                    case InventoryTypes.Result:
                        ChangeInventoryVisualSwitchSecondType(armCraft.CraftResult, data.Second.Type, data);
                        break;
                    case InventoryTypes.Chest:
                        ChangeInventoryVisualSwitchSecondType(chest.ChestInventory.InventorySlots, data.Second.Type, data);
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
            var armCraft = game.Player.Inventory as ArmCraft;
            if (IsInventoryOpen && armCraft.Craft(game.Crafts2by2))
            {
                visual.ChangeCraftImages(game.Player.Inventory);
            }
        }
    }
}
