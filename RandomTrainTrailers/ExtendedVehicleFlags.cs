namespace RandomTrainTrailers
{
    // This is a bit of a cheat and may stop working at some point.
    // It is an extension to Vehicle.Flags2, used to convey some mod-specific info to the rendering process.
    public enum ExtendedVehicleFlags
    {
        NoLights = 0x10000000,
        Reserved1 = 0x20000000,
        Reserved2 = 0x40000000,
    }
}
