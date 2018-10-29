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
        };

        /// <summary>
        /// Fallback table in case we can't assign the correct goods type
        /// </summary>
        public static readonly CargoFlags[][] ResourceFallback = {
            new[] {CargoFlags.Oil, CargoFlags.Petrol, CargoFlags.Goods, CargoFlags.Food, CargoFlags.Ore, CargoFlags.Coal, CargoFlags.Grain, CargoFlags.Lumber, CargoFlags.Logs, CargoFlags.Metals, CargoFlags.Mail},
            new[] {CargoFlags.Petrol, CargoFlags.Oil, CargoFlags.Goods, CargoFlags.Food, CargoFlags.Ore, CargoFlags.Coal, CargoFlags.Grain, CargoFlags.Lumber, CargoFlags.Logs, CargoFlags.Metals, CargoFlags.Mail},
            new[] {CargoFlags.Ore, CargoFlags.Coal, CargoFlags.Goods, CargoFlags.Food, CargoFlags.Oil, CargoFlags.Petrol, CargoFlags.Grain, CargoFlags.Lumber, CargoFlags.Logs, CargoFlags.Metals, CargoFlags.Mail},
            new[] {CargoFlags.Coal, CargoFlags.Ore, CargoFlags.Goods, CargoFlags.Food, CargoFlags.Oil, CargoFlags.Petrol, CargoFlags.Grain, CargoFlags.Lumber, CargoFlags.Logs, CargoFlags.Metals, CargoFlags.Mail},
            new[] {CargoFlags.Logs, CargoFlags.Lumber, CargoFlags.Goods, CargoFlags.Food, CargoFlags.Ore, CargoFlags.Coal, CargoFlags.Grain, CargoFlags.Petrol, CargoFlags.Oil, CargoFlags.Metals, CargoFlags.Mail},
            new[] {CargoFlags.Lumber, CargoFlags.Logs, CargoFlags.Goods, CargoFlags.Food, CargoFlags.Ore, CargoFlags.Coal, CargoFlags.Grain, CargoFlags.Petrol, CargoFlags.Oil, CargoFlags.Metals, CargoFlags.Mail},
            new[] {CargoFlags.Grain, CargoFlags.Food, CargoFlags.Goods, CargoFlags.Mail, CargoFlags.Lumber, CargoFlags.Coal, CargoFlags.Logs, CargoFlags.Petrol, CargoFlags.Oil, CargoFlags.Metals, CargoFlags.Ore},
            new[] {CargoFlags.Food, CargoFlags.Goods, CargoFlags.Grain, CargoFlags.Mail, CargoFlags.Lumber, CargoFlags.Coal, CargoFlags.Logs, CargoFlags.Petrol, CargoFlags.Oil, CargoFlags.Metals, CargoFlags.Ore},
            new[] {CargoFlags.Goods, CargoFlags.Food, CargoFlags.Grain, CargoFlags.Ore, CargoFlags.Lumber, CargoFlags.Coal, CargoFlags.Logs, CargoFlags.Petrol, CargoFlags.Oil, CargoFlags.Metals, CargoFlags.Mail},
            new[] {CargoFlags.Mail, CargoFlags.Goods, CargoFlags.Food, CargoFlags.Ore, CargoFlags.Lumber, CargoFlags.Coal, CargoFlags.Logs, CargoFlags.Petrol, CargoFlags.Oil, CargoFlags.Grain, CargoFlags.Metals},
            new[] {CargoFlags.Metals, CargoFlags.Goods, CargoFlags.Ore, CargoFlags.Food, CargoFlags.Lumber, CargoFlags.Coal, CargoFlags.Logs, CargoFlags.Petrol, CargoFlags.Oil, CargoFlags.Grain, CargoFlags.Mail},
        };

        public CargoParcel(ushort buildingID, bool incoming, byte transferType, ushort transferSize, Vehicle.Flags flags)
        {
            this.transferSize = transferSize;
            this.building = buildingID;
            this.flags = CargoFlags.None;

            switch((TransferType)transferType)
            {
                case TransferType.Oil:
                    this.flags |= CargoFlags.Oil;
                    break;
                case TransferType.Ore:
                    this.flags |= CargoFlags.Ore;
                    break;
                case TransferType.Logs:
                    this.flags |= CargoFlags.Logs;
                    break;
                case TransferType.Grain:
                    this.flags |= CargoFlags.Grain;
                    break;
                case TransferType.Petrol:
                    this.flags |= CargoFlags.Petrol;
                    break;
                case TransferType.Coal:
                    this.flags |= CargoFlags.Coal;
                    break;
                case TransferType.Lumber:
                    this.flags |= CargoFlags.Lumber;
                    break;
                case TransferType.Food:
                    this.flags |= CargoFlags.Food;
                    break;
                case TransferType.Goods:
                    this.flags |= CargoFlags.Goods;
                    break;
                case TransferType.Mail:
                    this.flags |= CargoFlags.Mail;
                    break;
                case TransferType.UnsortedMail:
                    this.flags |= CargoFlags.Mail;
                    break;
                case TransferType.SortedMail:
                    this.flags |= CargoFlags.Mail;
                    break;
                case TransferType.IncomingMail:
                    this.flags |= CargoFlags.Mail;
                    break;
                case TransferType.OutgoingMail:
                    this.flags |= CargoFlags.Mail;
                    break;
                case TransferType.AnimalProducts:
                    this.flags |= CargoFlags.Food;
                    break;
                case TransferType.Flours:
                    this.flags |= CargoFlags.Grain;
                    break;
                case TransferType.Paper:
                    this.flags |= CargoFlags.Goods;
                    break;
                case TransferType.PlanedTimber:
                    this.flags |= CargoFlags.Lumber;
                    break;
                case TransferType.Petroleum:
                    this.flags |= CargoFlags.Petrol;
                    break;
                case TransferType.Plastics:
                    this.flags |= CargoFlags.Goods;
                    break;
                case TransferType.Glass:
                    this.flags |= CargoFlags.Goods;
                    break;
                case TransferType.Metals:
                    this.flags |= CargoFlags.Metals;
                    break;
                case TransferType.LuxuryProducts:
                    this.flags |= CargoFlags.Goods;
                    break;
                default:
                    // Changed to use RTT error logging
                    Util.LogError("Unexpected transfer type: " + Enum.GetName(typeof(TransferType), transferType));
                    break;
            }
        }
    }
}
