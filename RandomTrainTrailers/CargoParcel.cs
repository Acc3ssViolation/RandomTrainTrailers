using System;

using TransferType = TransferManager.TransferReason;
namespace RandomTrainTrailers
{ 
    [Flags]
    public enum CargoFlags
    {
        None    = 0,
        Oil     = 1,
        Petrol  = 2,
        Ore     = 4,
        Coal    = 8,
        Logs    = 16,
        Lumber  = 32,
        Grain   = 64,
        Food    = 128,
        Goods   = 256,
        Mail    = 512,
        Metals  = 1024,
        AnimalProducts = 2048,
    }

    public struct CargoParcel
    {
        public ushort building;
        public ushort transferSize;
        public CargoFlags flags;

        public int ResourceType => LowestFlagToIndex(flags);

        public static int LowestFlagToIndex(CargoFlags flags)
        {
            int index = 0;

            while(index <= ResourceTypes.Length)
            {
                if(((0x01 << index) & (int)flags) > 0)
                {
                    return index;
                }
                index++;
            }

            return -1;
        }

        public static readonly CargoFlags[] ResourceTypes =
        {
            CargoFlags.Oil,
            CargoFlags.Petrol,
            CargoFlags.Ore,
            CargoFlags.Coal,
            CargoFlags.Logs,
            CargoFlags.Lumber,
            CargoFlags.Grain,
            CargoFlags.Food,
            CargoFlags.Goods,
            CargoFlags.Mail,
            CargoFlags.Metals,
            CargoFlags.AnimalProducts,
        };

        /// <summary>
        /// Fallback table in case we can't assign the correct goods type
        /// </summary>
        public static readonly CargoFlags[][] ResourceFallback = {
            new[] {CargoFlags.Oil, CargoFlags.Petrol, CargoFlags.Goods, CargoFlags.Food, CargoFlags.Ore, CargoFlags.Coal, CargoFlags.Grain, CargoFlags.Lumber, CargoFlags.Logs, CargoFlags.Metals, CargoFlags.Mail, CargoFlags.AnimalProducts},
            new[] {CargoFlags.Petrol, CargoFlags.Oil, CargoFlags.Goods, CargoFlags.Food, CargoFlags.Ore, CargoFlags.Coal, CargoFlags.Grain, CargoFlags.Lumber, CargoFlags.Logs, CargoFlags.Metals, CargoFlags.Mail, CargoFlags.AnimalProducts},
            new[] {CargoFlags.Ore, CargoFlags.Coal, CargoFlags.Goods, CargoFlags.Food, CargoFlags.Oil, CargoFlags.Petrol, CargoFlags.Grain, CargoFlags.Lumber, CargoFlags.Logs, CargoFlags.Metals, CargoFlags.Mail, CargoFlags.AnimalProducts},
            new[] {CargoFlags.Coal, CargoFlags.Ore, CargoFlags.Goods, CargoFlags.Food, CargoFlags.Oil, CargoFlags.Petrol, CargoFlags.Grain, CargoFlags.Lumber, CargoFlags.Logs, CargoFlags.Metals, CargoFlags.Mail, CargoFlags.AnimalProducts},
            new[] {CargoFlags.Logs, CargoFlags.Lumber, CargoFlags.Goods, CargoFlags.Food, CargoFlags.Ore, CargoFlags.Coal, CargoFlags.Grain, CargoFlags.Petrol, CargoFlags.Oil, CargoFlags.Metals, CargoFlags.Mail, CargoFlags.AnimalProducts},
            new[] {CargoFlags.Lumber, CargoFlags.Logs, CargoFlags.Goods, CargoFlags.Food, CargoFlags.AnimalProducts, CargoFlags.Ore, CargoFlags.Coal, CargoFlags.Grain, CargoFlags.Petrol, CargoFlags.Oil, CargoFlags.Metals, CargoFlags.Mail},
            new[] {CargoFlags.Grain, CargoFlags.Food, CargoFlags.Goods, CargoFlags.AnimalProducts, CargoFlags.Mail, CargoFlags.Lumber, CargoFlags.Coal, CargoFlags.Logs, CargoFlags.Petrol, CargoFlags.Oil, CargoFlags.Metals, CargoFlags.Ore},
            new[] {CargoFlags.Food, CargoFlags.Goods, CargoFlags.AnimalProducts, CargoFlags.Grain, CargoFlags.Mail, CargoFlags.Lumber, CargoFlags.Coal, CargoFlags.Logs, CargoFlags.Petrol, CargoFlags.Oil, CargoFlags.Metals, CargoFlags.Ore},
            new[] {CargoFlags.Goods, CargoFlags.Food, CargoFlags.Grain, CargoFlags.AnimalProducts, CargoFlags.Ore, CargoFlags.Lumber, CargoFlags.Coal, CargoFlags.Logs, CargoFlags.Petrol, CargoFlags.Oil, CargoFlags.Metals, CargoFlags.Mail},
            new[] {CargoFlags.Mail, CargoFlags.Goods, CargoFlags.Food, CargoFlags.AnimalProducts, CargoFlags.Ore, CargoFlags.Lumber, CargoFlags.Coal, CargoFlags.Logs, CargoFlags.Petrol, CargoFlags.Oil, CargoFlags.Grain, CargoFlags.Metals},
            new[] {CargoFlags.Metals, CargoFlags.Goods, CargoFlags.Ore, CargoFlags.Food, CargoFlags.AnimalProducts, CargoFlags.Lumber, CargoFlags.Coal, CargoFlags.Logs, CargoFlags.Petrol, CargoFlags.Oil, CargoFlags.Grain, CargoFlags.Mail},
            new[] {CargoFlags.AnimalProducts, CargoFlags.Food, CargoFlags.Goods, CargoFlags.Grain, CargoFlags.Mail, CargoFlags.Lumber, CargoFlags.Coal, CargoFlags.Logs, CargoFlags.Petrol, CargoFlags.Oil, CargoFlags.Metals, CargoFlags.Ore},
        };

        public CargoParcel(ushort buildingID, bool incoming, byte transferType, ushort transferSize, Vehicle.Flags flags)
        {
            this.transferSize = transferSize;
            this.building = buildingID;
            this.flags = TransferToFlags((TransferType)transferType);
        }

        public static CargoFlags TransferToFlags(TransferType transfer)
        {
            switch(transfer)
            {
                case TransferType.Oil:
                    return CargoFlags.Oil;
                case TransferType.Ore:
                    return CargoFlags.Ore;
                case TransferType.Logs:
                    return CargoFlags.Logs;
                case TransferType.Grain:
                    return CargoFlags.Grain;
                case TransferType.Petrol:
                    return CargoFlags.Petrol;
                case TransferType.Coal:
                    return CargoFlags.Coal;
                case TransferType.Lumber:
                    return CargoFlags.Lumber;
                case TransferType.Food:
                    return CargoFlags.Food;
                case TransferType.Goods:
                    return CargoFlags.Goods;
                case TransferType.Mail:
                    return CargoFlags.Mail;
                case TransferType.UnsortedMail:
                    return CargoFlags.Mail;
                case TransferType.SortedMail:
                    return CargoFlags.Mail;
                case TransferType.IncomingMail:
                    return CargoFlags.Mail;
                case TransferType.OutgoingMail:
                    return CargoFlags.Mail;
                case TransferType.AnimalProducts:
                    return CargoFlags.AnimalProducts;
                case TransferType.Flours:
                    return CargoFlags.Grain;
                case TransferType.Paper:
                    return CargoFlags.Goods;
                case TransferType.PlanedTimber:
                    return CargoFlags.Lumber;
                case TransferType.Petroleum:
                    return CargoFlags.Petrol;
                case TransferType.Plastics:
                    return CargoFlags.Goods;
                case TransferType.Glass:
                    return CargoFlags.Goods;
                case TransferType.Metals:
                    return CargoFlags.Metals;
                case TransferType.LuxuryProducts:
                    return CargoFlags.Goods;
                default:
                    // Changed to use RTT error logging
                    Util.LogError("Unexpected transfer type: " + Enum.GetName(typeof(TransferType), transfer));
                    return CargoFlags.Goods;
            }
        }

        public static byte FlagIndexToGateIndex(int flagIndex)
        {
            if(flagIndex == 0 || flagIndex == 1)        // Oil, Petrol
            {
                return 6;
            }
            else if(flagIndex == 2 || flagIndex == 3)   // Ore, Coal
            {
                return 7;
            }
            else if(flagIndex == 4) // Logs
            {
                return 4;
            }
            else if(flagIndex == 6) // Grain
            {
                return 3;
            }
            else if(flagIndex == 11) // AnimalProducts
            {
                return 2;
            }
            return 0;   // Generic goods and all other
        }

        public static string FlagIndexToName(int flagIndex)
        {
            return ResourceTypes[flagIndex].ToString();
        }

        public static string TransferToName(byte transfer)
        {
            return ((TransferType)transfer).ToString();
        }

        public static byte GetEmptyGateIndex(byte gateIndex)
        {
            if(gateIndex == 0) { return 1; }
            if(gateIndex == 4) { return 5; }
            if(gateIndex == 7) { return 8; }
            return gateIndex;
        }
    }
}
