using System;
using System.Security.Policy;
using PixelRPG;

public class GameControls
{
    private GameModel game;
    private GameVisual visual;
    private bool isPlayerDamaged;
    public readonly System.Windows.Forms.Timer worldTimer;
    private static Dictionary<char, Sides> turns = new Dictionary<char, Sides>()
    {
        {'A',Sides.Left },
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
    private Inventory InventoryOpen;
    public GameControls( GameModel game, GameVisual visual)
	{
        this.game = game;
        this.visual = visual;
        worldTimer = GetWorldTimer();
        isPlayerDamaged = false;
	}

    public System.Windows.Forms.Timer GetWorldTimer()
    {
        var timer = new System.Windows.Forms.Timer();
        timer.Interval = GameModel.peacefulMobMoveTick;
        timer.Tick += (sender, e) =>
        {
            var random = new Random();
            foreach (var entity in visual.CurrentViewedMobs.Values)
                switch (entity.Action)
                {
                    case EntityActionType.Peaceful:
                        RandomEntityMove(entity);
                        break;
                    case EntityActionType.Enemy:
                        MoveToPlayerAndAttack(entity);
                        break;
                }
            DecreaceSatiety();
            if (game.Player.Satiety < 5)
                if (random.NextDouble() < GameModel.ChangeHealthBecauseOfFoodChance)
                {
                    game.Player.DamageEntity(1);
                    ViewDamagedPlayer(timer);
                }
            if (game.Player.Satiety > 15 && game.Player.Health < game.Player.MaxHealth)
                if (random.NextDouble() < GameModel.ChangeHealthBecauseOfFoodChance)
                {
                    game.Player.IncreaseHealth(1);
                    game.Player.DecreaseSatiety(1);
                    visual.ChangePlayerHealthView();
                    visual.ChangePlayerFoodView();
                }
            if (random.NextDouble() < GameModel.IncreacingManaChance)
            {
                game.Player.IncreaseMana(1);
                visual.ChangePlayerManaView();
            }
            visual.GetWorldVisual(game.Player.Position);
            if (isPlayerDamaged)
            {
                ViewDamagedPlayer(timer);
                isPlayerDamaged = false;
            }
        };
        return timer;
    }

    private void ViewDamagedPlayer(System.Windows.Forms.Timer timer)
    {
        visual.ViewDamageEffect(game.Player.Position);
        if (game.Player.Health == 0)
        {
            Death(game.Player);
            visual.OpenMenu(MenuType.Escape);
            timer.Stop();
        }
    }

    private void DecreaceSatiety()
    {
        var random = new Random();
        if (random.NextDouble() <GameModel.DecreacingSatietyChance)
        {
            game.Player.DecreaseSatiety(1);
            visual.ChangePlayerFoodView();
        }
    }

    private void Death(Entity entity)
    {
        game.Mobs.Remove(entity.Position);
        game.Chests[entity.Position] = new Chest(entity.Inventory, game.Player.Inventory);
        game.World[entity.Position.X, entity.Position.Y] = game.AllWorldElements["Heap"];
        visual.ChangeOneCellByWorldCoords(entity.Position.X, entity.Position.Y, game.World[entity.Position.X, entity.Position.Y]);
    }

    private void SetDirectionOrMove(Entity entity, Sides side)
    {
        var point = moves[side];
        if (entity.Direction == side)
            game.MoveEntity(entity,new Point(entity.Position.X + point.X, entity.Position.Y + point.Y));
        else
            entity.SetDirection(side);
    }

    private void MoveToPlayerAndAttack(Entity entity)
    {
        var random = new Random();
        var vertical = entity.Position.Y - game.Player.Position.Y;
        var horisontal = entity.Position.X - game.Player.Position.X;
        if (random.NextDouble() < GameModel.enemyMobMoveChance)
        {
            if (vertical > 0)
                SetDirectionOrMove(entity, Sides.Up);
            else if (vertical < 0)
                SetDirectionOrMove(entity, Sides.Down);
            else if (horisontal > 0)
                SetDirectionOrMove(entity, Sides.Left);
            else if (horisontal < 0)
                SetDirectionOrMove(entity, Sides.Right);
            var point = moves[entity.Direction];
            if (game.Player.Position == new Point(entity.Position.X + point.X, entity.Position.Y + point.Y))
            {
                game.Player.DamageEntity(entity.BaseDamage);
                visual.ChangePlayerHealthView();
                isPlayerDamaged = true;
            }
        }
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
        var frontPoint = new Point(game.Player.Position.X + moves[game.Player.Direction].X, game.Player.Position.Y + moves[game.Player.Direction].Y);
        var chest = game.Chests.ContainsKey(frontPoint) ? game.Chests[frontPoint] : null;
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
            visual.OpenMenu(MenuType.Escape);
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
                    Death(entity);
            }
        }
        if (controlChar == (char)Keys.P)
        {
            if (frontElement.Type == WorldElementType.Empty && !game.Mobs.ContainsKey(frontPoint) && game.Player.Inventory.InventorySlots[0, 0].Type != WorldElementType.Empty)
            {
                if (game.Player.Inventory.InventorySlots[0, 0].ParentBlockName != null)
                    game.World[frontPoint.X, frontPoint.Y] = game.AllWorldElements[game.Player.Inventory.InventorySlots[0, 0].ParentBlockName];
                else
                    game.World[frontPoint.X, frontPoint.Y] = game.Player.Inventory.InventorySlots[0, 0];
                game.Player.Inventory.InventorySlots[0, 0] = game.AllWorldElements["Empty"];
                visual.ChangeOneCellByWorldCoords(frontPoint.X, frontPoint.Y, game.World[frontPoint.X, frontPoint.Y]);
                visual.ChangeCurrentInventorySlot(game.Player.Inventory.InventorySlots[0, 0]);
            }
            else if (!IsInventoryOpen && game.Chests.ContainsKey(frontPoint))
            {
                InventoryOpen = game.Chests[frontPoint];
                visual.OpenInventory(InventoryOpen);
                IsInventoryOpen = true;
            }
        }
        if (controlChar == (char)Keys.M)
        {
            var spellCentrePoint = new Point(game.Player.Position.X + moves[game.Player.Direction].X * (game.Player.CurrentSpell.DamageRange.GetLength(0) / 2 + 1),
                game.Player.Position.Y + moves[game.Player.Direction].Y * (game.Player.CurrentSpell.DamageRange.GetLength(1) / 2 + 1));
            var spellLeftTopPoint = new Point(spellCentrePoint.X - game.Player.CurrentSpell.DamageRange.GetLength(0) / 2,
                spellCentrePoint.Y - game.Player.CurrentSpell.DamageRange.GetLength(1) / 2);
            if (game.Player.DecreaseMana(game.Player.CurrentSpell.ManaWasting))
                for (int i = 0; i < game.Player.CurrentSpell.DamageRange.GetLength(0); i++)
                    for (int j = 0; j < game.Player.CurrentSpell.DamageRange.GetLength(1); j++)
                        if (game.Player.CurrentSpell.DamageRange[i, j] != 0)
                        {
                            var currentPoint = new Point(spellLeftTopPoint.X + i, spellLeftTopPoint.Y + j);
                            if (game.Mobs.ContainsKey(currentPoint))
                                game.Mobs[currentPoint].DamageEntity(game.Player.CurrentSpell.DamageRange[i, j]);
                            visual.ViewDamageEffect(currentPoint);
                        }
            visual.ChangePlayerManaView();
            visual.ChangePlayerHealthView();
        }
    }

    private void ChangeInventoryIfChar(char controlChar)
    {
        var frontPoint = new Point(game.Player.Position.X + moves[game.Player.Direction].X, game.Player.Position.Y + moves[game.Player.Direction].Y);
        if (controlChar == (char)Keys.I)
            if (IsInventoryOpen)
            {
                visual.CloseInventory(InventoryOpen);
                IsInventoryOpen = false;
                if (game.Chests.ContainsKey(frontPoint) && game.Chests[frontPoint].IsEmpty())
                {
                    game.World[frontPoint.X, frontPoint.Y] = game.AllWorldElements["Empty"];
                    visual.ChangeOneCellByWorldCoords(frontPoint.X, frontPoint.Y, game.World[frontPoint.X, frontPoint.Y]);
                }
            }
            else
            {
                InventoryOpen = game.Player.Inventory;
                visual.OpenInventory(InventoryOpen);
                IsInventoryOpen = true;
            }
        if (controlChar == (char)Keys.J)
        {
            var armCraft = game.Player.Inventory as ArmCraft;
            var chest = game.Chests.ContainsKey(frontPoint)? game.Chests[frontPoint]:null;
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
        if (controlChar == (char)Keys.P)
        {
            var armCraft = game.Player.Inventory as ArmCraft;
            if (armCraft.SelectedSlots.Count  > 0)
            {
                var data = armCraft.SelectedSlots.ElementAt(armCraft.SelectedSlots.Count-1);
                var element = armCraft.InventorySlots[data.X, data.Y];
                if (element.Type == WorldElementType.Food)
                {
                    armCraft.SelectedSlots.Dequeue();
                    armCraft.InventorySlots[data.X, data.Y] = game.AllWorldElements["Empty"];
                    game.Player.IncreaseSatiety((int)element.SatietyBonus);
                    visual.ChangeInventoryCell(armCraft.SelectedSlots.Count+1, armCraft.InventorySlots[data.X, data.Y]);
                    visual.ChangePlayerFoodView();
                }
            }
        }
    }
}
