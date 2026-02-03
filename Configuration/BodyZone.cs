namespace CDoT.Configuration
{
    public enum BodyZone
    {
        Unknown = 0,
        Throat = 10,
        Head = 20,
        Neck = 30,
        Torso = 40,
        Arm = 50,
        Leg = 60,
        Dismemberment = 100
    }

    public static class BodyZoneExtensions
    {
        public static string GetDisplayName(this BodyZone zone)
        {
            switch (zone)
            {
                case BodyZone.Throat: return "Throat";
                case BodyZone.Head: return "Head";
                case BodyZone.Neck: return "Neck";
                case BodyZone.Torso: return "Torso";
                case BodyZone.Arm: return "Arm";
                case BodyZone.Leg: return "Leg";
                case BodyZone.Dismemberment: return "Dismemberment";
                default: return "Unknown";
            }
        }

        public static float GetDefaultMultiplier(this BodyZone zone)
        {
            switch (zone)
            {
                case BodyZone.Throat: return 3.0f;
                case BodyZone.Head: return 2.0f;
                case BodyZone.Neck: return 2.5f;
                case BodyZone.Torso: return 1.0f;
                case BodyZone.Arm: return 0.5f;
                case BodyZone.Leg: return 0.6f;
                case BodyZone.Dismemberment: return 2.5f;
                default: return 1.0f;
            }
        }

        public static float GetDefaultDuration(this BodyZone zone)
        {
            switch (zone)
            {
                case BodyZone.Throat: return 8.0f;
                case BodyZone.Head: return 6.0f;
                case BodyZone.Neck: return 7.0f;
                case BodyZone.Torso: return 5.0f;
                case BodyZone.Arm: return 4.0f;
                case BodyZone.Leg: return 4.5f;
                case BodyZone.Dismemberment: return 10.0f;
                default: return 5.0f;
            }
        }

        public static float GetDefaultDamagePerTick(this BodyZone zone)
        {
            switch (zone)
            {
                case BodyZone.Throat: return 5.0f;
                case BodyZone.Head: return 3.0f;
                case BodyZone.Neck: return 4.0f;
                case BodyZone.Torso: return 2.0f;
                case BodyZone.Arm: return 1.0f;
                case BodyZone.Leg: return 1.5f;
                case BodyZone.Dismemberment: return 6.0f;
                default: return 2.0f;
            }
        }

        public static int GetDefaultStackLimit(this BodyZone zone)
        {
            switch (zone)
            {
                case BodyZone.Throat: return 3;
                case BodyZone.Head: return 3;
                case BodyZone.Neck: return 3;
                case BodyZone.Torso: return 5;
                case BodyZone.Arm: return 4;
                case BodyZone.Leg: return 4;
                case BodyZone.Dismemberment: return 1;
                default: return 3;
            }
        }
    }
}
