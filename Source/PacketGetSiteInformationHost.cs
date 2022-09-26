using System;
namespace EdcHost;
using System.Collections.Generic;

internal class PacketGetSiteInformationHost : Packet
{
    private readonly byte PacketId = 0x01;

    private int _obstacleListLength;
    private List<Barrier> _obstacleList;
    private GameStageType _currentGameStage;
    private int _duration;
    private int _ownChargingPilesLength;
    private List<Dot> _ownChargingPiles;
    private int _opponentChargingPilesLength;
    private List<Dot> _opponentChargingPiles;

    /// <summary>
    /// Construct a GetSiteInformation packet with fields.
    /// </summary>
    /// <remarks>
    /// There is no field in this type of packets.
    /// </remarks>
    public PacketGetSiteInformationHost(
        List<Barrier> obstacleList,
        GameStageType currentGameStage,
        int duration,
        List<Dot> ownChargingPiles,
        List<Dot> opponentChargingPiles)
    {
        this._obstacleListLength = obstacleList.Count;
        this._obstacleList = obstacleList;
        this._currentGameStage = currentGameStage;
        this._duration = duration;
        this._ownChargingPilesLength = ownChargingPiles.Count;
        this._ownChargingPiles = opponentChargingPiles;
        this._opponentChargingPilesLength = opponentChargingPiles.Count;
        this._opponentChargingPiles = opponentChargingPiles;
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
        byte[] data = Packet.ExtractPacketData(bytes);

        byte packetId = bytes[0];
        if (packetId != this.PacketId)
        {
            throw new Exception("The packet ID is incorrect.");
        }

        int currentIndex = 0;
        // Obstacle data
        this._obstacleListLength = BitConverter.ToInt32(data, currentIndex);
        currentIndex += 4;
        // Get the information from 
        for (int i = 0; i < this._obstacleListLength; i++)
        {
            Dot left_up = new Dot(BitConverter.ToInt32(data, currentIndex), BitConverter.ToInt32(data, currentIndex + 4));
            Dot right_down = new Dot(BitConverter.ToInt32(data, currentIndex + 8), BitConverter.ToInt32(data, currentIndex + 12));

            this._obstacleList.Add(new Barrier(left_up, right_down));
            currentIndex += 4 * 4;
        }

        // Gamestage 
        this._currentGameStage = (GameStageType)BitConverter.ToInt32(data, currentIndex);
        currentIndex += 4;

        this._duration = BitConverter.ToInt32(data, currentIndex);
        currentIndex += 8;

        // Get the information of owncharging piles
        this._ownChargingPilesLength = BitConverter.ToInt32(data, currentIndex);
        currentIndex += 4;

        for (int i = 0; i < this._ownChargingPilesLength; i++)
        {
            this._ownChargingPiles.Add(new Dot(BitConverter.ToInt32(data, currentIndex), BitConverter.ToInt32(data, currentIndex + 4)));
            currentIndex += 4 * 2;
        }

        // Get the information of opponent's charging piles
        this._opponentChargingPilesLength = BitConverter.ToInt32(data, currentIndex);
        currentIndex += 4;

        for (int i = 0; i < this._opponentChargingPilesLength; i++)
        {
            this._opponentChargingPiles.Add(new Dot(BitConverter.ToInt32(data, currentIndex), BitConverter.ToInt32(data, currentIndex + 4)));
            currentIndex += 4 * 2;
        }

    }

    public override byte[] GetBytes()
    {
        // Compute the length of the data
        int dataLength = (
            4 +                                    // this._obstacleListLength
            this._obstacleListLength * 16 +
            1 * 4 +                                // this._currentGameStage
            1 * 8 +                                // this._duration
            4 +                                    // this._ownChargingPilesLength
            this._ownChargingPilesLength * 8 +
            4 +                                    // this._opponentChargingPilesLength
            this._opponentChargingPilesLength * 8
        );
        // Initialize the data array
        var data = new byte[dataLength];

        int currentIndex = 0;

        // Obstacle
        BitConverter.GetBytes(this._obstacleListLength).CopyTo(data, currentIndex);
        currentIndex += 4;

        for (int i = 0; i < this._obstacleListLength; i++)
        {
            // 2 Dots —— 16 Bytes per Obstacle
            BitConverter.GetBytes(this._obstacleList[i].TopLeftPosition.x).CopyTo(data, currentIndex);
            BitConverter.GetBytes(this._obstacleList[i].TopLeftPosition.y).CopyTo(data, currentIndex + 4);
            BitConverter.GetBytes(this._obstacleList[i].BottomRightPosition.x).CopyTo(data, currentIndex + 8);
            BitConverter.GetBytes(this._obstacleList[i].BottomRightPosition.y).CopyTo(data, currentIndex + 12);
            currentIndex += 4 * 4;
        }

        // Gamestage 
        BitConverter.GetBytes((int)this._currentGameStage).CopyTo(data, currentIndex);
        currentIndex += 4;

        BitConverter.GetBytes(this._duration).CopyTo(data, currentIndex);
        currentIndex += 8;

        // encode the information of owncharging piles
        BitConverter.GetBytes(this._ownChargingPilesLength).CopyTo(data, currentIndex);
        currentIndex += 4;

        for (int i = 0; i < this._ownChargingPilesLength; i++)
        {
            // 2 Dots —— 16 Bytes per Obstacle
            for (int intNumber = 0; intNumber < 2; intNumber++)
            {
                BitConverter.GetBytes(this._ownChargingPilesLength).CopyTo(data, currentIndex);
                currentIndex += 4;
            }
        }

        // encode the information of opponent's charging piles
        BitConverter.GetBytes(this._ownChargingPilesLength).CopyTo(data, currentIndex);
        currentIndex += 4;

        for (int i = 0; i < this._opponentChargingPilesLength; i++)
        {
            // 2 Dots —— 16 Bytes per Obstacle
            for (int intNumber = 0; intNumber < 2; intNumber++)
            {
                BitConverter.GetBytes(this._opponentChargingPilesLength).CopyTo(data, currentIndex);
                currentIndex += 4;
            }
        }

        // write the data's information into the header
        var header = GeneratePacketHeader(this.PacketId, data);

        var bytes = new byte[header.Length + data.Length];
        header.CopyTo(bytes, 0);
        data.CopyTo(bytes, header.Length);

        return bytes;
    }

}