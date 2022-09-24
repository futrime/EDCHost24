using System;
namespace EdcHost;
using System.Collections.Generic;

internal class PacketGetSiteInformationHost : Packet
{
    private readonly byte PacketId = 0x01;

    private int ObstacleListLength;
    private List<Wall> ObstacleList;
    private GameStage CurrentGameStage;
    private double CurrentTime;
    private int OwnChargingPilesLength;
    private List<Dot> OwnChargingPiles;
    private int OpponentChargingPilesLength;
    private List<Dot> OpponentChargingPiles;

    /// <summary>
    /// Construct a GetSiteInformation packet with fields.
    /// </summary>
    /// <remarks>
    /// There is no field in this type of packets.
    /// </remarks>
    public PacketGetSiteInformationHost(
        int ObstacleListLength,
        List<Wall> ObstacleList,
        GameStage CurrentGameStage,
        double CurrentTime,
        int OwnChargingPilesLength,
        List<Dot> OwnChargingPiles,
        int OpponentChargingPilesLength,
        List<Dot> OpponentChargingPiles)
    {
        this.ObstacleListLength = ObstacleListLength;
        this.ObstacleList = ObstacleList;
        this.CurrentGameStage = CurrentGameStage;
        this.CurrentTime = CurrentTime;
        this.OwnChargingPilesLength = OwnChargingPilesLength;
        this.OwnChargingPiles = OpponentChargingPiles;
        this.OpponentChargingPilesLength = OpponentChargingPilesLength;
        this.OpponentChargingPiles = OpponentChargingPiles;
    }

    /// <summary>
    /// Construct a GetSiteInformation packet with a raw byte array.
    /// </summary>
    /// <param name="bytes">The raw byte array</param>
    /// <exception cref="ArgumentException">
    /// The raw byte array violates the rules.
    /// </exception>
    public PacketGetSiteInformationHost(byte[] bytes)
    {
        // Validate the packet and extract data
        Packet.ExtractPacketData(bytes);

        byte packetId = bytes[0];
        if (packetId != this.PacketId)
        {
            throw new Exception("The packet ID is incorrect.");
        }

        int CurrentIndex = 6;
        // Obstacle data
        this.ObstacleListLength = BitConverter.ToInt32(bytes, 6);
        CurrentIndex += 4;
        // Get the information from 
        for (int i = 0; i < this.ObstacleListLength; i++)
        {
            Dot left_up = new Dot(BitConverter.ToInt32(bytes, CurrentIndex), BitConverter.ToInt32(bytes, CurrentIndex + 4));
            Dot right_down = new Dot(BitConverter.ToInt32(bytes, CurrentIndex + 8), BitConverter.ToInt32(bytes, CurrentIndex + 16));

            this.ObstacleList.Add(new Wall(left_up, right_down));
            CurrentIndex += 4 * 4;
        }

        // Gamestage 
        this.CurrentGameStage = (GameStage)BitConverter.ToInt32(bytes, CurrentIndex);
        CurrentIndex += 4;

        this.CurrentTime = BitConverter.ToDouble(bytes, CurrentIndex);
        CurrentIndex += 8;

        // Get the information of owncharging piles
        this.OwnChargingPilesLength = BitConverter.ToInt32(bytes, CurrentIndex);
        CurrentIndex += 4;

        for (int i = 0; i < this.OwnChargingPilesLength; i++)
        {
            this.OwnChargingPiles.Add(new Dot(BitConverter.ToInt32(bytes, CurrentIndex), BitConverter.ToInt32(bytes, CurrentIndex + 4)));
            CurrentIndex += 4 * 2;
        }

        // Get the information of opponent's charging piles
        this.OpponentChargingPilesLength = BitConverter.ToInt32(bytes, CurrentIndex);
        CurrentIndex += 4;

        for (int i = 0; i < this.OpponentChargingPilesLength; i++)
        {
            this.OpponentChargingPiles.Add(new Dot(BitConverter.ToInt32(bytes, CurrentIndex), BitConverter.ToInt32(bytes, CurrentIndex + 4)));
            CurrentIndex += 4 * 2;
        }

    }

    public override byte[] GetBytes()
    {
        // Compute the length of the data
        int DataLength = (
            this.ObstacleListLength * 16 +
            1 * 4 +                                // this.CurrentGameStage
            1 * 8 +                                // Duration
            this.OwnChargingPilesLength * 8 +
            this.OpponentChargingPilesLength * 8
        );
        // Initialize the data array
        var data = new byte[DataLength];

        int CurrentIndex = 0;

        // Obstacle
        BitConverter.GetBytes(this.ObstacleListLength).CopyTo(data, CurrentIndex);
        CurrentIndex += 4;

        for (int i = 0; i < this.ObstacleListLength; i++)
        {
            // 2 Dots —— 16 Bytes per Obstacle
            BitConverter.GetBytes(this.ObstacleList[i].w1.x).CopyTo(data, CurrentIndex);
            CurrentIndex += 4 * 4;
            BitConverter.GetBytes(this.ObstacleList[i].w1.y).CopyTo(data, CurrentIndex);
            BitConverter.GetBytes(this.ObstacleList[i].w2.x).CopyTo(data, CurrentIndex);
            BitConverter.GetBytes(this.ObstacleList[i].w2.y).CopyTo(data, CurrentIndex);


        }

        // Gamestage 
        BitConverter.GetBytes((int)this.CurrentGameStage).CopyTo(data, CurrentIndex);
        CurrentIndex += 4;

        BitConverter.GetBytes(this.CurrentTime).CopyTo(data, CurrentIndex);
        CurrentIndex += 8;

        // encode the information of owncharging piles
        BitConverter.GetBytes(this.OwnChargingPilesLength).CopyTo(data, CurrentIndex);
        CurrentIndex += 4;

        for (int i = 0; i < this.OwnChargingPilesLength; i++)
        {
            // 2 Dots —— 16 Bytes per Obstacle
            for (int intNumber = 0; intNumber < 2; intNumber++)
            {
                BitConverter.GetBytes(this.OwnChargingPilesLength).CopyTo(data, CurrentIndex);
                CurrentIndex += 4;
            }
        }

        // encode the information of opponent's charging piles
        BitConverter.GetBytes(this.OwnChargingPilesLength).CopyTo(data, CurrentIndex);
        CurrentIndex += 4;

        for (int i = 0; i < this.OpponentChargingPilesLength; i++)
        {
            // 2 Dots —— 16 Bytes per Obstacle
            for (int intNumber = 0; intNumber < 2; intNumber++)
            {
                BitConverter.GetBytes(this.OpponentChargingPilesLength).CopyTo(data, CurrentIndex);
                CurrentIndex += 4;
            }
        }

        // write the data's information into the header
        var header = new byte[6];


        header[0] = this.PacketId;
        BitConverter.GetBytes(data.Length).CopyTo(header, 1);
        header[5] = Packet.CalculateChecksum(data);

        var bytes = new byte[header.Length + data.Length];
        header.CopyTo(bytes, 0);
        data.CopyTo(bytes, header.Length);

        return bytes;
    }

}