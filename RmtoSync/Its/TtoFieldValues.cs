namespace RmtoSync.Its;

/// <summary>TTOInfo field code values from ITS guide chapters 3-4 and samples.</summary>
public static class TtoFieldValues
{
    public static class Allowed
    {
        public const long Violation = 0;
        public const long Permitted = 1;
    }

    public static class VehicleClass
    {
        public const long Light = 1;
        public const long Heavy = 2;
    }

    public static class SpeedType
    {
        public const long None = 0;
        public const long Instant = 1;
        public const long Average = 2;
        public const long InstantAndAverage = 3;
    }

    public static class WrongDirection
    {
        public const long Correct = 0;
        public const long Opposite = 1;
    }
}
