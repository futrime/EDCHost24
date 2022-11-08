using System;
namespace EdcHost;
using System.Collections.Generic;

public class PacketGetGameInformationHost : Packet
{
    public const byte PacketId = 0x01;
    private GameStageType _gameStage;
    private List<Barrier> _barrierList;
    private int _duration;
    private List<Dot> _ownChargingPiles;  // class Dot is sufficient for use. So I don't use type List<ChargingPile>
    private List<Dot> _opponentChargingPiles;

    /// <summary>
    /// Construct a GetSiteInformation packet with fields.
    /// </summary>
    public PacketGetGameInformationHost(
        GameStageType gameStage,
        List<Barrier> barrierList,
        int duration,
        List<Dot> ownChargingPiles,
        List<Dot> opponentChargingPiles)
    {
        this._barrierList = barrierList;
        this._gameStage = gameStage;
        this._duration = duration;
        this._ownChargingPiles = opponentChargingPiles;
        this._opponentChargingPiles = opponentChargingPiles;
    }

    /// <summary>
    /// Construct a GetGameInformationHost packet from a raw byte array.
    /// </summary>
    /// <param name="bytes">The raw byte array.</param>
    public PacketGetGameInformationHost(byte[] bytes)
    {
        // Validate the packet and extract data
        byte[] data = Packet.ExtractPacketData(bytes);

        byte packetId = bytes[0];
        if (packetId != PacketGetGameInformationHost.PacketId)
        {
            throw new Exception("The packet ID is incorrect.");
        }

        int currentIndex = 0;

        // Gamestage 
        this._gameStage = (GameStageType)(data[currentIndex]);
        currentIndex += 1;

        // Obstacle data
        byte barrierListLength = data[currentIndex];
        currentIndex += 1;

        // Get the information from barrier list
        this._barrierList = new List<Barrier>() { };
        for (int i = 0; i < barrierListLength; i++)
        {
            Dot left_up = new Dot(BitConverter.ToInt32(data, currentIndex), BitConverter.ToInt32(data, currentIndex + 4));
            Dot right_down = new Dot(BitConverter.ToInt32(data, currentIndex + 8), BitConverter.ToInt32(data, currentIndex + 12));

            this._barrierList.Add(new Barrier(left_up, right_down));
            currentIndex += 4 * 4;
        }

        this._duration = BitConverter.ToInt32(data, currentIndex);
        currentIndex += 4;

        // Get the information of owncharging piles
        byte ownChargingPilesLength = data[currentIndex];
        currentIndex += 1;

        this._ownChargingPiles = new List<Dot>() { };
        for (int i = 0; i < ownChargingPilesLength; i++)
        {
            this._ownChargingPiles.Add(new Dot(BitConverter.ToInt32(data, currentIndex), BitConverter.ToInt32(data, currentIndex + 4)));
            currentIndex += 4 * 2;
        }

        // Get the information of opponent's charging piles
        byte opponentChargingPilesLength = data[currentIndex];
        currentIndex += 1;

        this._opponentChargingPiles = new List<Dot>() { };
        for (int i = 0; i < opponentChargingPilesLength; i++)
        {
            this._opponentChargingPiles.Add(new Dot(BitConverter.ToInt32(data, currentIndex), BitConverter.ToInt32(data, currentIndex + 4)));
            currentIndex += 4 * 2;
        }

    }

    public override byte[] GetBytes()
    {
        // Compute the length of the data
        int dataLength = (
            1 +                                    // this._currentGameStage
            1 +                                    // this._obstacleListLength
            this._barrierList.Count * 16 +
            1 * 4 +                                // this._duration
            1 +                                    // this._ownChargingPilesLength
            this._ownChargingPiles.Count * 8 +
            1 +                                    // this._opponentChargingPilesLength
            this._opponentChargingPiles.Count * 8
        );
        // Initialize the data array
        var data = new byte[dataLength];

        int currentIndex = 0;


        // Gamestage 
        data[currentIndex] = ((byte)this._gameStage);
        currentIndex += 1;

        // Barrier
        if (this._barrierList.Count > 0xff)
        {
            throw new ArgumentException("Barrier length is larger than 0xff");
        }
        data[currentIndex] = ((byte)this._barrierList.Count);
        currentIndex += 1;

        for (int i = 0; i < this._barrierList.Count; i++)
        {
            // 2 Dots —— 16 Bytes per Obstacle
            BitConverter.GetBytes(this._barrierList[i].TopLeftPosition.X).CopyTo(data, currentIndex);
            BitConverter.GetBytes(this._barrierList[i].TopLeftPosition.Y).CopyTo(data, currentIndex + 4);
            BitConverter.GetBytes(this._barrierList[i].BottomRightPosition.X).CopyTo(data, currentIndex + 8);
            BitConverter.GetBytes(this._barrierList[i].BottomRightPosition.Y).CopyTo(data, currentIndex + 12);
            currentIndex += 4 * 4;
        }

        BitConverter.GetBytes(this._duration).CopyTo(data, currentIndex);
        currentIndex += 4;

        // Encode the information of owncharging piles
        // If the count is too large, throw an exception
        if (this._ownChargingPiles.Count > 0xff)
        {
            throw new ArgumentException("Length of own charging piles is larger than 0xff");
        }
        data[currentIndex] = ((byte)this._ownChargingPiles.Count);
        currentIndex += 1;

        for (int i = 0; i < this._ownChargingPiles.Count; i++)
        {
            // 1 Dot —— 8 Bytes per ChargingPile
            BitConverter.GetBytes(this._ownChargingPiles[i].X).CopyTo(data, currentIndex);
            BitConverter.GetBytes(this._ownChargingPiles[i].Y).CopyTo(data, currentIndex + 4);
            currentIndex += 8;
        }

        // Encode the information of opponent's charging piles
        // If the count is too large, throw an exception
        if (this._opponentChargingPiles.Count > 0xff)
        {
            throw new ArgumentException("Length of opponent charging piles is larger than 0xff");
        }

        data[currentIndex] = ((byte)this._opponentChargingPiles.Count);
        currentIndex += 1; // ZYR FIXED BUG in 2022-11-9

        for (int i = 0; i < this._opponentChargingPiles.Count; i++)
        {
            // 1 Dot —— 8 Bytes per ChargingPile
            BitConverter.GetBytes(this._opponentChargingPiles[i].X).CopyTo(data, currentIndex);
            BitConverter.GetBytes(this._opponentChargingPiles[i].Y).CopyTo(data, currentIndex + 4);
            currentIndex += 8;
        }

        // write the data's information into the header
        var header = GeneratePacketHeader(PacketGetGameInformationHost.PacketId, data);

        var bytes = new byte[header.Length + data.Length];
        header.CopyTo(bytes, 0);
        data.CopyTo(bytes, header.Length);

        return bytes;
    }

    public override byte GetPacketId()
    {
        return PacketGetGameInformationHost.PacketId;
    }

}