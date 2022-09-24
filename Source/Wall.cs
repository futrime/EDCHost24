namespace EdcHost;
public class Wall
{
    public Dot w1;
    public Dot w2;
    public Wall(Dot left_up, Dot right_down)
    {
        w1 = left_up;
        w2 = right_down;
    }
    public Wall(Wall aWall)
    {
        w1 = aWall.w1;
        w2 = aWall.w2;
    }
}