using System;
using System.Security.Policy;
using PixelRPG;

public class GameControls
{
    private GameModel game;
    private GameVisual visual;
    private bool isPlayerDamaged;
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
        isPlayerDamaged = false;
	}

    public void GetWorldTimer()
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
                ViewDamagedPlayer();
            }
        if (game.Player.Satiety > 15 && game.Player.Health < game.Player.MaxHealth)
            if (random.NextDouble() < GameModel.ChangeHealthBecauseOfFoodChance)
            {
                game.Player.IncreaseHealth(1 * game.Player.RegenerationExp / 100);
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
            ViewDamagedPlayer();
            isPlayerDamaged = false;
        }
    }

    private void ViewDamagedPlayer()
    {
        if (!IsInventoryOpen)
            visual.ViewEffect(game.Player.Position,SpellType.Damage);
        if (game.Player.Health == 0)
        {
            Death(game.Player);
            visual.OpenMenu(MenuType.Main);
            visual.StopWorldTimer();
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
        game.World[entity.Position.X, entity.Position.Y] = GameModel.AllWorldElements["Heap"];
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
                game.Player.DamageEntity(entity.BaseDamage * entity.DamageExp / 100);
                entity.IncreaseDamageExp(entity.BaseDamage);
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


    public void CheckKeyCommands(char symbol)
    {
        var controlChar = char.ToUpper(symbol);
        EscapeIfChar(controlChar);
        MoveIfChar(controlChar);
        WorldInteractionIfChar(controlChar);
        ChangeInventoryIfChar(controlChar);
        ChangeCurrentSpellIfChar(controlChar);
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

    public void ChangeInventoryVisualSwitchSecondType(MagicSpell[,] first, InventoryTypes second,
    (bool IsComplete, (int X, int Y, InventoryTypes Type) First, (int X, int Y, InventoryTypes Type) Second) data)
    {
        var spellTableInventory = game.SpellTableSaved;
        switch (second)
        {
            case InventoryTypes.SpellInventory:
                visual.ChangeInventoryCell(1, first[data.First.X, data.First.Y]);
                visual.ChangeInventoryCell(2, spellTableInventory.SavedSpells[data.Second.X, data.Second.Y]);
                break;
            case InventoryTypes.SpellSlots:
                visual.ChangeInventoryCell(1, first[data.First.X, data.First.Y]);
                visual.ChangeInventoryCell(2, spellTableInventory.PlayerSpells[data.Second.X, data.Second.Y]);
                break;
            case InventoryTypes.Result:
                visual.ChangeInventoryCell(1, first[data.First.X, data.First.Y]);
                visual.ChangeInventoryCell(2, spellTableInventory.Result[data.Second.X, data.Second.Y]);
                break;
        }
    }

    private void ChangeCurrentSpellIfChar(char controlChar)
    {
        var i = game.Player.CurrentSpellIndex;
        if (int.TryParse(controlChar.ToString(), out i))
        {
            game.Player.ChangeCurrentSpellIndex(i-1);
            visual.ChangeCurrentMagicSpell(game.Player.CurrentSpell);
        }
    }

    private void EscapeIfChar(char controlChar)
    {
        if (controlChar == (char)Keys.Escape)
        {
            visual.StopWorldTimer();
            visual.OpenMenu(MenuType.Escape);
        }
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
                if (frontElement.IsChest)
                {
                    game.World[frontPoint.X, frontPoint.Y] = GameModel.AllWorldElements["Heap"];
                    game.Chests[frontPoint].ChestInventory.AddInFirstEmptySlot(frontElement.Drop);
                }
                else
                    game.World[frontPoint.X, frontPoint.Y] = frontElement.Drop;
                visual.ChangeOneCellByWorldCoords(frontPoint.X, frontPoint.Y, game.World[frontPoint.X, frontPoint.Y]);
            }
            else if (game.Mobs.ContainsKey(frontPoint))
            {
                var entity = game.Mobs[frontPoint];
                entity.DamageEntity(game.Player.Inventory.InventorySlots[0, 0].Damage * game.Player.DamageExp / 100);
                game.Player.IncreaseDamageExp(game.Player.Inventory.InventorySlots[0, 0].Damage);
                visual.ViewEffect(entity.Position,SpellType.Damage);
                if (entity.Health == 0)
                    Death(entity);
            }
        }
        if (controlChar == (char)Keys.P)
        {
            if (!IsInventoryOpen && frontElement.Type == WorldElementType.Empty && !game.Mobs.ContainsKey(frontPoint) && game.Player.Inventory.InventorySlots[0, 0].Type != WorldElementType.Empty)
            {
                if (game.Player.Inventory.InventorySlots[0, 0].ParentBlockName != null)
                    game.World[frontPoint.X, frontPoint.Y] = GameModel.AllWorldElements[game.Player.Inventory.InventorySlots[0, 0].ParentBlockName];
                else
                    game.World[frontPoint.X, frontPoint.Y] = game.Player.Inventory.InventorySlots[0, 0];
                if (game.Player.Inventory.InventorySlots[0,0].IsChest)
                    game.Chests[frontPoint] = new Chest(new Inventory(), game.Player.Inventory);
                game.Player.Inventory.InventorySlots[0, 0] = GameModel.AllWorldElements["Empty"];
                visual.ChangeOneCellByWorldCoords(frontPoint.X, frontPoint.Y, game.World[frontPoint.X, frontPoint.Y]);
                visual.ChangeCurrentInventorySlot(game.Player.Inventory.InventorySlots[0, 0]);
            }
            else if (!IsInventoryOpen && game.Chests.ContainsKey(frontPoint))
            {
                InventoryOpen = game.Chests[frontPoint];
                visual.OpenInventory(InventoryOpen);
                IsInventoryOpen = true;
            }
            else if (!IsInventoryOpen && frontElement.Equals(GameModel.AllWorldElements["SpellTable"]))
            {
                visual.OpenSpellTableInterface();
                InventoryOpen = game.SpellTableSaved;
                IsInventoryOpen = true;
            }
        }
        if (controlChar == (char)Keys.M)
        {
            var spellCentrePoint = new Point(game.Player.Position.X + moves[game.Player.Direction].X * (game.Player.CurrentSpell.DamageRange.GetLength(0) / 2 + 1),
                game.Player.Position.Y + moves[game.Player.Direction].Y * (game.Player.CurrentSpell.DamageRange.GetLength(1) / 2 + 1));
            var spellLeftTopPoint = new Point(spellCentrePoint.X - game.Player.CurrentSpell.DamageRange.GetLength(0) / 2,
                spellCentrePoint.Y - game.Player.CurrentSpell.DamageRange.GetLength(1) / 2);
            if (game.Player.DecreaseMana(game.Player.CurrentSpell.ManaWasting / (game.Player.ManaExp / 100)))
            {
                game.Player.IncreaseManaExp(game.Player.CurrentSpell.ManaWasting);
                switch (game.Player.CurrentSpell.Type)
                {
                    case SpellType.Damage:
                        DoDamageSpell(spellLeftTopPoint);
                        break;
                    case SpellType.Find:
                        ViewWay(FindElement(game.Player.CurrentSpell.FindingElement)); 
                        break;
                }  
            }
            visual.ChangePlayerManaView();
            visual.ChangePlayerHealthView();
        }
    }

    private MyLinkedList FindElement(WorldElement el)
    {
        var queue = new Queue<MyLinkedList>();
        queue.Enqueue(new MyLinkedList(game.Player.Position) );
        var visited = new HashSet<Point>() { game.Player.Position };
        while (queue.Count != 0)
        {
            var listElement = queue.Dequeue();
            foreach (var point in moves.Values)
            {
                var newPoint = new Point(listElement.Last.Value.X + point.X, listElement.Last.Value.Y + point.Y);
                if (game.InBounds(newPoint) && game.IsStepablePoint(newPoint)
                && !visited.Contains(newPoint))
                {
                    visited.Add(newPoint);
                    var newElement = new MyLinkedList();
                    newElement.SetLast(listElement.Last, listElement.Count);
                    newElement.AddLast(newPoint);
                    queue.Enqueue(newElement);
                    if (game.World[newPoint.X,newPoint.Y].Equals(el))
                        return newElement;
                }
            }
        }
        return null;
    }

    private void ViewWay(MyLinkedList way)
    {
        if (way == null) return;
        var wayPoint = way.Last;
        while (wayPoint != null)
        {
            visual.ViewEffect(wayPoint.Value, SpellType.Find);
            wayPoint = wayPoint.Previous;
        }
    }

    private void DoDamageSpell(Point spellLeftTopPoint)
    {
        for (int i = 0; i < game.Player.CurrentSpell.DamageRange.GetLength(0); i++)
            for (int j = 0; j < game.Player.CurrentSpell.DamageRange.GetLength(1); j++)
                if (game.Player.CurrentSpell.DamageRange[i, j] != 0)
                {
                    var currentPoint = new Point(spellLeftTopPoint.X + i, spellLeftTopPoint.Y + j);
                    if (game.Mobs.ContainsKey(currentPoint))
                    {
                        game.Mobs[currentPoint].DamageEntity(game.Player.CurrentSpell.DamageRange[i, j]);
                        if (game.Mobs[currentPoint].Health == 0)
                            Death(game.Mobs[currentPoint]);
                    }
                    visual.ViewEffect(currentPoint, SpellType.Damage);
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
                if (game.Chests.ContainsKey(frontPoint) && game.World[frontPoint.X, frontPoint.Y].Equals(GameModel.AllWorldElements["Heap"]) && game.Chests[frontPoint].IsEmpty())
                {
                    game.World[frontPoint.X, frontPoint.Y] = GameModel.AllWorldElements["Empty"];
                    visual.ChangeOneCellByWorldCoords(frontPoint.X, frontPoint.Y, game.World[frontPoint.X, frontPoint.Y]);
                    game.Chests.Remove(frontPoint);
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
            var spellTableInventory = game.SpellTableSaved;
            (bool IsComplete, (int X, int Y, InventoryTypes Type) First, (int X, int Y, InventoryTypes Type) Second) data = default;
            if (InventoryOpen.Equals(armCraft))
                data = armCraft.ChangeSelectedSlots();
            if (InventoryOpen.Equals(chest))
                data = chest.ChangeSelectedSlots();
            if (InventoryOpen.Equals(spellTableInventory))
                data = spellTableInventory.ChangeSelectedSlots();
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
                        if (InventoryOpen.Equals(armCraft))
                            ChangeInventoryVisualSwitchSecondType(armCraft.CraftResult, data.Second.Type, data);
                        else
                            ChangeInventoryVisualSwitchSecondType(spellTableInventory.Result, data.Second.Type, data);
                        break;
                    case InventoryTypes.Chest:
                        ChangeInventoryVisualSwitchSecondType(chest.ChestInventory.InventorySlots, data.Second.Type, data);
                        break;
                    case InventoryTypes.SpellInventory:
                        ChangeInventoryVisualSwitchSecondType(spellTableInventory.SavedSpells, data.Second.Type, data);
                        break;
                    case InventoryTypes.SpellSlots:
                        ChangeInventoryVisualSwitchSecondType(spellTableInventory.PlayerSpells, data.Second.Type, data);
                        break;
                }
            if (InventoryOpen.Equals(spellTableInventory))
                for (int i = 0;i<spellTableInventory.PlayerSpells.GetLength(1);i++)
                    game.Player.Spells[i] = spellTableInventory.PlayerSpells[0,i];
            game.Player.ChangeCurrentSpellIndex(game.Player.CurrentSpellIndex);
            visual.ChangeCurrentMagicSpell(game.Player.CurrentSpell);
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
                    armCraft.InventorySlots[data.X, data.Y] = GameModel.AllWorldElements["Empty"];
                    game.Player.IncreaseSatiety((int)element.SatietyBonus * game.Player.SatietyExp / 100);
                    game.Player.IncreaseSatietyExp((int)element.SatietyBonus);
                    visual.ChangeInventoryCell(armCraft.SelectedSlots.Count+1, armCraft.InventorySlots[data.X, data.Y]);
                    visual.ChangePlayerFoodView();
                }
            }
        }
    }
}
