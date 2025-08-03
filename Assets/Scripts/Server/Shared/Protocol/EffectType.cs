namespace Shared.Protocol
{
    // we use 3bits for the packet type
    public enum EffectType : byte
    {
        Death = 0,
        DriftBoost = 1, // 1 bit
        Ready = 2,
        Charging = 3, // 2 bits
        StopDriftChargeAndReady = 4,
        ResetTrail = 5,
        TurnOffTrail = 6
    }
}
