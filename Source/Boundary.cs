namespace EdcHost;
public class Boundary
{
    static private int mMap = 254;
    static private int mSize = 28;
    static private int mEntrance = 36;
    static private int mWidth = 2;
    // 双实线内侧到边界的距离
    static private int size1 = mSize + mWidth;
    // 双实线的长度
    static private int height = (mMap - 2 * mSize - mEntrance) / 2;

    private static Wall[] mWall = new Wall[4]{
        new Wall(new Dot(mSize, mSize), new Dot(mSize, mSize + height)),
         new Wall(new Dot(size1, size1), new Dot(size1, mSize + height)),
        new Wall(new Dot(mSize, mSize), new Dot(mSize + height, mSize)),
         new Wall(new Dot(size1, size1), new Dot(mSize + height, size1))
    };

    // 构造函数
    //四个参数分别是：地图的边长，分界线到地图边缘的最短距离，入口的大小，双实线的宽度
    //这里，由于围墙具有对称性，所以我们只计算四段围墙，计算时，将小车坐标对称变换到第一象限
    public Boundary()
    {
        Wall[] mWall = new Wall[4];
        mMap = 254;
        mSize = 28;
        mEntrance = 36;
        mWidth = 2;
        // 双实线内侧到边界的距离
        int size1 = mSize + mWidth;
        // 双实线的长度
        int height = (mMap - 2 * mSize - mEntrance) / 2;
        mWall[0] = new Wall(new Dot(mSize, mSize), new Dot(mSize, mSize + height));
        mWall[1] = new Wall(new Dot(size1, size1), new Dot(size1, mSize + height));
        mWall[2] = new Wall(new Dot(mSize, mSize), new Dot(mSize + height, mSize));
        mWall[3] = new Wall(new Dot(size1, size1), new Dot(mSize + height, size1));
    }

    public static bool isCollided(Dot CarPos, int radius = 8)
    {
        if (CarPos.x > mMap / 2)
        {
            CarPos.x = mMap - CarPos.x;
        }
        if (CarPos.y > mMap / 2)
        {
            CarPos.y = mMap - CarPos.y;
        }
        foreach (Wall wall in mWall)
        {
            if (Utility.DistanceL(wall, CarPos) < radius)
            {
                return true;
            }
        }
        return false;
    }
}