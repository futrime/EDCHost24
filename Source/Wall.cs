namespace EdcHost;
public class Wall
{
    public Dot w1;
    public Dot w2;
    public Wall(Dot iw1, Dot iw2)
    {
        w1 = iw1;
        w2 = iw2;
    }
    public Wall(Wall aWall)
    {
        w1 = aWall.w1;
        w2 = aWall.w2;
    }
}