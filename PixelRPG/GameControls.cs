using System;
using System.Security.Policy;
using PixelRPG;

public class GameControls
{
    private GameModel game;
    private GameVisual visual;
    public GameControls( GameModel game, GameVisual visual)
	{
        this.game = game;
        this.visual = visual;
	}

    public void Move(Point newPosition)=>
        visual.GetWorldVisual(newPosition);


    public void SetKeyCommands(TextBox keyBar)
    {
        keyBar.KeyPress += (sender, e) =>
        {
            Point newPosition = new Point(-1,-1);
            if (AreCharsEqual(e.KeyChar, (char)Keys.A)) newPosition = new Point(game.PlayerPosition.X, game.PlayerPosition.Y - 1);
            if (AreCharsEqual(e.KeyChar, (char)Keys.S)) newPosition = new Point(game.PlayerPosition.X + 1, game.PlayerPosition.Y);
            if (AreCharsEqual(e.KeyChar, (char)Keys.D)) newPosition = new Point(game.PlayerPosition.X, game.PlayerPosition.Y + 1);
            if (AreCharsEqual(e.KeyChar, (char)Keys.W)) newPosition = new Point(game.PlayerPosition.X - 1, game.PlayerPosition.Y);
            if (game.InBounds(newPosition))
            {
                Move(newPosition);
                game.SetPlayerPosition(newPosition);
            }
        };
    }

    public static bool AreCharsEqual(char char1, char char2) =>
        char1 == char2 || char.ToUpper(char1) == char2 || char1==char.ToUpper(char2);
}
