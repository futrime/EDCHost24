using System;
namespace EdcHost;
using System.Collections.Generic;

public class PacketGetGameInformationHost : Packet
{
    public const byte PacketId = 0x01;
    private GameStageType _gameStage;
    private List<Barrier> _barrierList;
    private long _duration;
    private List<Dot> _ownChargingPiles;  // class Dot is sufficient for use. So I don't use type List<ChargingPile>
    private List<Dot> _opponentChargingPiles;

    /// <summary>
    /// Construct a GetSiteInformation packet with fields.
    /// </summary>
    public PacketGetGameInformationHost(
        GameStageType gameStage,
        List<Barrier> barrierList,
        long duration,
        List<Dot> ownChargingPiles,
        List<Dot> opponentChargingPiles)
    {
        this._barrierList = barrierList;
        this._gameStage = gameStage;
        this._duration = duration;
        this._ownChargingPiles = opponentChargingPiles;
        this._opponentChargingPiles = opponentChargingPiles;
    }

    public override byte[] GetBytes()
    {
        // Compute the length of the data
        int dataLength = (
            1 * 4 +                                // this._currentGameStage
            4 +                                    // this._obstacleListLength
            this._barrierList.Count * 16 +
            1 * 8 +                                // this._duration
            4 +                                    // this._ownChargingPilesLength
            this._ownChargingPiles.Count * 8 +
            4 +                                    // this._opponentChargingPilesLength
            this._opponentChargingPiles.Count * 8
        );
        // Initialize the data array
        var data = new byte[dataLength];

        int currentIndex = 0;


        // Gamestage 
        BitConverter.GetBytes((int)this._gameStage).CopyTo(data, currentIndex);
        currentIndex += 4;

        // Barrier
        BitConverter.GetBytes(this._barrierList.Count).CopyTo(data, currentIndex);
        currentIndex += 4;

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
        currentIndex += 8;

        // encode the information of owncharging piles
        BitConverter.GetBytes(this._ownChargingPiles.Count).CopyTo(data, currentIndex);
        currentIndex += 4;

        for (int i = 0; i < this._ownChargingPiles.Count; i++)
        {
            // 2 Dots —— 16 Bytes per Obstacle
            for (int intNumber = 0; intNumber < 2; intNumber++)
            {
                BitConverter.GetBytes(this._ownChargingPiles.Count).CopyTo(data, currentIndex);
                currentIndex += 4;
            }
        }

        // encode the information of opponent's charging piles
        BitConverter.GetBytes(this._ownChargingPiles.Count).CopyTo(data, currentIndex);
        currentIndex += 4;

        for (int i = 0; i < this._opponentChargingPiles.Count; i++)
        {
            // 2 Dots —— 16 Bytes per Obstacle
            for (int intNumber = 0; intNumber < 2; intNumber++)
            {
                BitConverter.GetBytes(this._opponentChargingPiles.Count).CopyTo(data, currentIndex);
                currentIndex += 4;
            }
        }

        // write the data's information into the header
        var header = GeneratePacketHeader(PacketGetGameInformationHost.PacketId, data);

        var bytes = new byte[header.Length + data.Length];
        header.CopyTo(bytes, 0);
        data.CopyTo(bytes, header.Length);

        return bytes;
    }
    /// <summary>
    /// Construct a GetSiteInformation packet with a raw byte array.
    /// </summary>
    /// <param name="bytes">The raw byte array</param>
    /// <exception cref="ArgumentException">
    /// The raw byte array violates the rules.
    /// </exception>
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
        this._gameStage = (GameStageType)BitConverter.ToInt32(data, currentIndex);
        currentIndex += 4;

        // Obstacle data
        int barrierListLength = BitConverter.ToInt32(data, currentIndex);
        currentIndex += 4;
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
        currentIndex += 8;

        // Get the information of owncharging piles
        int ownChargingPilesLength = BitConverter.ToInt32(data, currentIndex);
        currentIndex += 4;

        this._ownChargingPiles = new List<Dot>() { };
        for (int i = 0; i < ownChargingPilesLength; i++)
        {
            this._ownChargingPiles.Add(new Dot(BitConverter.ToInt32(data, currentIndex), BitConverter.ToInt32(data, currentIndex + 4)));
            currentIndex += 4 * 2;
        }

        // Get the information of opponent's charging piles
        int opponentChargingPilesLength = BitConverter.ToInt32(data, currentIndex);
        currentIndex += 4;

        this._opponentChargingPiles = new List<Dot>() { };
        for (int i = 0; i < opponentChargingPilesLength; i++)
        {
            this._opponentChargingPiles.Add(new Dot(BitConverter.ToInt32(data, currentIndex), BitConverter.ToInt32(data, currentIndex + 4)));
            currentIndex += 4 * 2;
        }

    }
    public override byte GetPacketId()
    {
        return PacketGetGameInformationHost.PacketId;
    }

}