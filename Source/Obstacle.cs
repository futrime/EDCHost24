using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace EdcHost;

public class Obstacle
{
    static public List<string> LabyName = null;
    public const int MAX_WALL_NUM = 8;
    // 障碍物的最小边长
    public const int OBSTACLE_MIN_LENGTH = 10;
    // 障碍物的最大边长
    public const int OBSTACLE_MAX_LENGTH = 16;
    //包裹离障碍物的最小距离
    public const int PACKAGE_MIN_DISTANCE_FROM_OBSTACLE = 25;
    public const int OBSTACLE_MIN_DISTANCE_FROM_OBSTACLE = 25;

    static public Wall[] mpWallList = null;
    static public string FileNameNow;

    // 障碍物是否已经被设置
    static public bool IsLabySet;

    // 默认构造函数
    public Obstacle()
    {
        //开局就随机生成8个
        IsLabySet = true;
        // 默认的障碍物数量是8个
        mpWallList = new Wall[MAX_WALL_NUM];
        // 随机构造障碍物
        int time = (int)MathF.Abs((int)System.DateTime.Now.Ticks);

        Random random = new Random(time);

        //保证障碍物与包裹有一定距离的判定函数
        bool AwayFromPackages(int centerX, int centerY)
        {
            foreach (Package pkg in PackageList.mPackageList)
            {
                if (pkg != null)
                {
                    //判断与起点距离
                    if (Math.Sqrt((centerX - pkg.mDeparture.x) * (centerX - pkg.mDeparture.x) + (centerY - pkg.mDeparture.y) * (centerY - pkg.mDeparture.y)) < PACKAGE_MIN_DISTANCE_FROM_OBSTACLE)
                    {
                        return false;
                    }
                    //判断与终点距离
                    if (Math.Sqrt((centerX - pkg.mDestination.x) * (centerX - pkg.mDestination.x) + (centerY - pkg.mDestination.y) * (centerY - pkg.mDestination.y)) < PACKAGE_MIN_DISTANCE_FROM_OBSTACLE)
                    {
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("null package in Obstacle.cs");
                }
            }
            return true;
        }
        bool AwayFromObstacles(int centerX, int centerY)
        {
            foreach (Wall wall in mpWallList)
            {
                if (wall != null)
                {
                    int currentCenterX = (wall.w1.x + wall.w2.x) / 2;
                    int currentCenterY = (wall.w1.y + wall.w2.y) / 2;

                    //判断与障碍物的距离
                    if (Math.Sqrt((centerX - currentCenterX) * (centerX - currentCenterX) +
                    (centerY - currentCenterY) * (centerY - currentCenterY)) < OBSTACLE_MIN_DISTANCE_FROM_OBSTACLE)
                    {
                        return false;
                    }
                }
                // else
                // {
                //     Console.WriteLine("null obstacle in Obstacle.cs");
                // }
            }
            return true;
        }
        for (int i = 0; i < MAX_WALL_NUM; i++)
        {
            //左上角的点(x1,y1)
            int x1 = 0, y1 = 0, width = 0, height = 0;

            //循环，直到与包裹&&其它障碍物距离够大才退出
            do
            {
                x1 = random.Next() % (Game.MAX_SIZE - OBSTACLE_MAX_LENGTH);
                y1 = random.Next() % (Game.MAX_SIZE - OBSTACLE_MAX_LENGTH);
                width = random.Next(OBSTACLE_MIN_LENGTH, OBSTACLE_MAX_LENGTH);
                height = random.Next(OBSTACLE_MIN_LENGTH, OBSTACLE_MAX_LENGTH);
            }
            while (!AwayFromPackages(x1 + width / 2, y1 + height / 2) ||
            !AwayFromObstacles(x1 + width / 2, y1 + height / 2));

            mpWallList[i] = new Wall(new Dot(x1, y1), new Dot(x1 + width, y1 + height));
        }
        LabyName = new List<string>();
    }

    // 从文本读取障碍物信息
    public void ReadFromFile(string FileName)
    {
        try
        {
            IsLabySet = false;
            TextReader reader = File.OpenText("labyrinth/" + FileName);
            for (int i = 0; i < MAX_WALL_NUM; i++)
            {
                string text = reader.ReadLine();
                string[] bits = text.Split(' ');
                int x1 = int.Parse(bits[0]);
                int y1 = int.Parse(bits[1]);
                int x2 = int.Parse(bits[2]);
                int y2 = int.Parse(bits[3]);
                mpWallList[i] = new Wall(new Dot(x1, y1), new Dot(x2, y2));
            }
            // 障碍物成功设置

            IsLabySet = true;

            Debug.WriteLine("Labyrinth Created from text.");
        }
        catch (ArgumentException)
        {
            MessageBox.Show("无效的障碍物文件路径");
        }
        catch (FileNotFoundException)
        {
            MessageBox.Show("不存在指定的障碍物文件");
        }
        catch (NotSupportedException)
        {
            MessageBox.Show("文件路径格式无效");
        }
        FileNameNow = FileName;
    }

    public void GetLabyName()
    {
        // 将障碍物文件名列表清空
        LabyName.Clear();

        // 绑定到指定的文件夹目录
        DirectoryInfo dir = new DirectoryInfo("labyrinth");

        // 检索表示当前目录的文件和子目录
        FileSystemInfo[] fsinfos = dir.GetFileSystemInfos();

        // 遍历检索的文件和子目录
        foreach (FileSystemInfo fsinfo in fsinfos)
        {
            Console.WriteLine(fsinfo.Name);

            // 将得到的文件名放入到list中
            LabyName.Add(fsinfo.Name);
        }
    }

    // 圆角矩形碰撞检测
    private static bool CollideWall(Wall wall, Dot CarPos, int radius)
    {
        //碰撞范围应该是圆角矩形，车可能在上下左右;也可能在左上，右上，左下，右下，这两种必须分开讨论
        bool InRange(int min, int max, int number)
        {
            return min <= number && max >= number;
        }
        int width = wall.w2.x - wall.w1.x;
        int height = wall.w2.y - wall.w1.y;

        //首先是车在矩形上下的情况
        if (InRange(0, width, (CarPos.x - wall.w1.x)))
        {
            if (Math.Abs(CarPos.y - (wall.w1.y + height / 2.0f)) < height / 2.0f + radius)
            {
                return true;
            }
        }
        //然后是车在矩形左右的情况
        else if (InRange(0, height, (CarPos.y - wall.w1.y)))
        {
            if (Math.Abs(CarPos.x - (wall.w1.x + width / 2.0f)) < width / 2.0f + radius)
            {
                return true;
            }
        }
        //接下来判断左上，右上，左下，右下的情况
        else
        {
            //以左上角的顶点为划分接线，分清楚CarPos到底和四个点中的哪个点比较距离
            bool bigger_than_w1_x = CarPos.x > wall.w1.x;
            bool bigger_than_w1_y = CarPos.y > wall.w1.y;

            Dot dot_to_be_compared = new Dot(
                wall.w1.x + (bigger_than_w1_x ? 1 : 0) * width,
                wall.w1.y + (bigger_than_w1_y ? 1 : 0) * height
                );

            if (Utilities.DistanceP(dot_to_be_compared, CarPos) < radius)
            {
                return true;
            }
        }
        //未进入障碍物
        return false;
    }
    // 判断是否与障碍发生碰撞
    public static bool isCollided(Dot CarPos, int radius = 0)
    {
        if (radius < 0)
        {
            radius = 0;
        }
        foreach (Wall wall in mpWallList)
        {
            if (CollideWall(wall, CarPos, radius))
            {
                return true;
            }
        }
        return false;
    }
}