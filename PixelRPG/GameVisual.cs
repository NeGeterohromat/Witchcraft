﻿using System;
using System.Data.Common;

namespace PixelRPG
{
	public class GameVisual
	{
		private GameModel game;
		public const int ViewFieldSize = 20;
		private WorldElement[,] CurrentWorldView = new WorldElement[ViewFieldSize, ViewFieldSize];
		public Dictionary<Point, Entity> CurrentViewedMobs { get; private set; }
        public GameVisual(GameModel game)
		{
			this.game = game;
            CurrentViewedMobs = new Dictionary<Point, Entity>();
        }

		public event Action<int,int,Image,PictureBoxSizeMode> ChangeOneCellView;
		public event Action<int,WorldElement> ChangeInventoryCellView;
        public event Action<WorldElement> ChangeCurrentInventorySlotView;
        public event Action<Inventory> OpenInventoryView;
        public event Action<Inventory> CloseInventoryView;
        public event Action<MenuType> OpenMenuView;
        public event Action<Inventory> ChangeCraftImagesView;
		public event Action<int, int, Image> ViewDamageAt;
		public event Action ChangeHealthView;
		public event Action ChangeFoodView;
		public event Action ChangeManaView;

		public void ChangePlayerManaView() => ChangeManaView();
		public void ChangePlayerFoodView() => ChangeFoodView();
		public void ChangePlayerHealthView() => ChangeHealthView();
        public void OpenInventory(Inventory inv) => OpenInventoryView(inv);
        public void CloseInventory(Inventory inv) => CloseInventoryView(inv);
        public void OpenMenu(MenuType type) => OpenMenuView(type);
        public void ChangeCraftImages(Inventory inv) => ChangeCraftImagesView(inv);

        public void ChangeInventoryCell(int number, WorldElement cell) =>
			ChangeInventoryCellView(number, cell);

		public void ChangeCurrentInventorySlot(WorldElement el) =>
			ChangeCurrentInventorySlotView(el);

		public void ViewDamageEffect(Point worldPoint)
		{
            var viewRow = worldPoint.X - game.Player.Position.X + ViewFieldSize / 2;
            var viewColumn = worldPoint.Y - game.Player.Position.Y + ViewFieldSize / 2;
			if (viewRow >= 0 && viewRow < ViewFieldSize && viewColumn >= 0 && viewColumn < ViewFieldSize)
				ViewDamageAt(viewRow, viewColumn, Image.FromFile(@"images/icons/damage.png"));
        }

        public void ChangeOneCellByWorldCoords(int row, int column, Entity mob) =>
			ChangeOneCellByWorldCoords(row, column, Image.FromFile(game.FileName(mob)),PictureBoxSizeMode.Zoom);
        


        public void ChangeOneCellByWorldCoords(int row, int column, WorldElement cell)
		{
            var viewRow = row - game.Player.Position.X + ViewFieldSize / 2;
            var viewColumn = column - game.Player.Position.Y + ViewFieldSize / 2;
            if (cell != null)
                CurrentWorldView[viewRow, viewColumn] = cell;
			ChangeOneCellByWorldCoords(row, column, Image.FromFile(game.FileName(cell)), cell.Type == WorldElementType.Block ? PictureBoxSizeMode.StretchImage : PictureBoxSizeMode.Zoom);
        }

        public void ChangeOneCellByWorldCoords(int row, int column, Image image,PictureBoxSizeMode mode)
		{
			var viewRow =row - game.Player.Position.X + ViewFieldSize/2;
			var viewColumn =column- game.Player.Position.Y + ViewFieldSize/2;
			if (viewRow >= 0 && viewRow < ViewFieldSize && viewColumn >= 0 && viewColumn < ViewFieldSize)
                ChangeOneCellView(viewRow, viewColumn, image,mode);			
			else
				throw new Exception("OutOfViewBounds");
		}

		public void ChangeOneCell(int row, int column, WorldElement cell)
		{
            CurrentWorldView[row, column] = cell;
			ChangeOneCellView(row,column,Image.FromFile(game.FileName(cell)),cell.Type==WorldElementType.Block?PictureBoxSizeMode.StretchImage:PictureBoxSizeMode.Zoom);
        }

        public void ChangeOneCell(int row, int column, Entity mob) =>
            ChangeOneCellView(row, column, Image.FromFile(game.FileName(mob)),PictureBoxSizeMode.Zoom);

        public Dictionary<Point, Entity> ViewMobs(Point viewStartPoint)
		{
			foreach (var mob in game.Mobs.Values)
				if (mob.Position.X >= viewStartPoint.X && mob.Position.Y >= viewStartPoint.Y
				&& mob.Position.X < viewStartPoint.X + ViewFieldSize && mob.Position.Y < viewStartPoint.Y + ViewFieldSize)
				{
					CurrentViewedMobs[new Point(mob.Position.X - viewStartPoint.X, mob.Position.Y - viewStartPoint.Y)] = mob;
					if (ChangeOneCellView != null)
						ChangeOneCellView(mob.Position.X - viewStartPoint.X, mob.Position.Y - viewStartPoint.Y, Image.FromFile(game.FileName(mob)),PictureBoxSizeMode.Zoom);
				};
			return CurrentViewedMobs;
		}

        public (WorldElement[,] Elements, Dictionary<Point, Entity> Mobs) GetWorldVisual(Point playerPosition)
		{
			var table = CurrentWorldView;
            var viewStartPoint = new Point(playerPosition.X - ViewFieldSize / 2, playerPosition.Y - ViewFieldSize / 2);
			for (int i = 0; i < ViewFieldSize; i++)
				for (int j = 0; j < ViewFieldSize; j++)
				{
					var row = i;
					var column = j;
					var newCell = game.InBounds(new Point(viewStartPoint.X + i, viewStartPoint.Y + j)) ? 
						game.World[viewStartPoint.X + i, viewStartPoint.Y + j] : game.OutOfBounds;
					if (!newCell.Equals(CurrentWorldView[i, j]) || CurrentViewedMobs.ContainsKey(new Point(i, j)))
					{
						table[i, j] = newCell;
                        if (ChangeOneCellView != null) ChangeOneCellView(row, column, Image.FromFile(game.FileName(table[row, column])), table[row, column].Type == WorldElementType.Block ? PictureBoxSizeMode.StretchImage : PictureBoxSizeMode.Zoom);
                    }				
				}
			CurrentViewedMobs.Clear();
			var mobList = ViewMobs(viewStartPoint);
            return (table,mobList);
        }
	}
}
