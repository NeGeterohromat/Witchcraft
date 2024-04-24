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
                visual.ChangeOneCell(GameVisual.ViewFieldSize / 2, GameVisual.ViewFieldSize / 2, new Player(game.Player.Type,game.Player.Position,newView));
            }
            else if (game.InBounds(newPosition) && game.IsStepablePoint(newPosition))
            {
                Move(newPosition);
                game.SetPlayerPosition(newPosition);
            }
        };
    }
}
