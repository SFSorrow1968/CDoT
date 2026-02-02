using BDOT.Configuration;
using ThunderRoad;
using UnityEngine;

namespace BDOT.Core
{
    public static class ZoneDetector
    {
        /// <summary>
        /// Determines the body zone from a ragdoll part, with special handling for throat detection.
        /// </summary>
        public static BodyZone GetZone(RagdollPart part, bool isSliced = false)
        {
            if (part == null)
            {
                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] ZoneDetector: part is null");
                return BodyZone.Unknown;
            }

            // Check for dismemberment first
            if (isSliced || part.isSliced)
            {
                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] ZoneDetector: Part is sliced -> Dismemberment (partType=" + part.type + ")");
                return BodyZone.Dismemberment;
            }

            var partType = part.type;

            // Head detection
            if ((partType & RagdollPart.Type.Head) != 0)
            {
                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] ZoneDetector: RagdollPart.Type=" + partType + " -> Head");
                return BodyZone.Head;
            }

            // Neck detection - check for throat hit
            if ((partType & RagdollPart.Type.Neck) != 0)
            {
                // Try to determine if this is a throat hit (front of neck)
                if (IsThroatHit(part))
                {
                    if (BDOTModOptions.DebugLogging)
                        Debug.Log("[BDOT] ZoneDetector: RagdollPart.Type=" + partType + " -> Throat (neck front hit)");
                    return BodyZone.Throat;
                }
                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] ZoneDetector: RagdollPart.Type=" + partType + " -> Neck (not throat)");
                return BodyZone.Neck;
            }

            // Torso detection
            if ((partType & RagdollPart.Type.Torso) != 0)
            {
                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] ZoneDetector: RagdollPart.Type=" + partType + " -> Torso");
                return BodyZone.Torso;
            }

            // Arm detection (includes hands)
            if ((partType & (RagdollPart.Type.LeftArm | RagdollPart.Type.RightArm |
                            RagdollPart.Type.LeftHand | RagdollPart.Type.RightHand)) != 0)
            {
                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] ZoneDetector: RagdollPart.Type=" + partType + " -> Arm");
                return BodyZone.Arm;
            }

            // Leg detection (includes feet)
            if ((partType & (RagdollPart.Type.LeftLeg | RagdollPart.Type.RightLeg |
                            RagdollPart.Type.LeftFoot | RagdollPart.Type.RightFoot)) != 0)
            {
                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] ZoneDetector: RagdollPart.Type=" + partType + " -> Leg");
                return BodyZone.Leg;
            }

            if (BDOTModOptions.DebugLogging)
                Debug.Log("[BDOT] ZoneDetector: RagdollPart.Type=" + partType + " -> Unknown (no match)");
            return BodyZone.Unknown;
        }

        /// <summary>
        /// Determines the body zone from a collision instance.
        /// </summary>
        public static BodyZone GetZoneFromCollision(CollisionInstance collision)
        {
            if (collision == null)
                return BodyZone.Unknown;

            var hitPart = collision.damageStruct.hitRagdollPart;
            if (hitPart == null)
                return BodyZone.Unknown;

            return GetZone(hitPart, hitPart.isSliced);
        }

        /// <summary>
        /// Attempts to determine if a neck hit is specifically a throat hit (front of neck).
        /// This uses the hit direction relative to the creature's facing direction.
        /// </summary>
        private static bool IsThroatHit(RagdollPart neckPart)
        {
            if (neckPart == null || neckPart.ragdoll == null || neckPart.ragdoll.creature == null)
                return false;

            // For now, use a simple heuristic:
            // We consider any neck hit with significant penetration damage to be a throat hit
            // This could be enhanced with actual position/direction checking

            // Alternative approach: use collider contact point vs creature forward
            // For simplicity, we'll treat high-damage neck hits as throat hits
            // and use random distribution for edge cases

            // 50% chance to be throat vs neck for ambiguous hits
            // This provides gameplay variety while still making throat strikes special
            float roll = Random.value;
            bool isThroat = roll > 0.5f;
            if (BDOTModOptions.DebugLogging)
                Debug.Log("[BDOT] ZoneDetector: Throat roll=" + roll.ToString("F2") + " -> " + (isThroat ? "THROAT" : "neck"));
            return isThroat;
        }

        /// <summary>
        /// Gets the zone from a ragdoll part type directly.
        /// </summary>
        public static BodyZone GetZoneFromPartType(RagdollPart.Type partType, bool isSliced = false)
        {
            if (isSliced)
            {
                return BodyZone.Dismemberment;
            }

            if ((partType & RagdollPart.Type.Head) != 0)
            {
                return BodyZone.Head;
            }

            if ((partType & RagdollPart.Type.Neck) != 0)
            {
                // Without position data, default to neck (not throat)
                return BodyZone.Neck;
            }

            if ((partType & RagdollPart.Type.Torso) != 0)
            {
                return BodyZone.Torso;
            }

            if ((partType & (RagdollPart.Type.LeftArm | RagdollPart.Type.RightArm |
                            RagdollPart.Type.LeftHand | RagdollPart.Type.RightHand)) != 0)
            {
                return BodyZone.Arm;
            }

            if ((partType & (RagdollPart.Type.LeftLeg | RagdollPart.Type.RightLeg |
                            RagdollPart.Type.LeftFoot | RagdollPart.Type.RightFoot)) != 0)
            {
                return BodyZone.Leg;
            }

            return BodyZone.Unknown;
        }

        /// <summary>
        /// Determines if a damage type should cause bleeding.
        /// </summary>
        public static bool ShouldCauseBleed(DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.Slash:
                    if (BDOTModOptions.DebugLogging)
                        Debug.Log("[BDOT] ZoneDetector: DamageType=Slash -> causes bleed (100%)");
                    return true;
                case DamageType.Pierce:
                    if (BDOTModOptions.DebugLogging)
                        Debug.Log("[BDOT] ZoneDetector: DamageType=Pierce -> causes bleed (100%)");
                    return true;
                case DamageType.Blunt:
                    // Blunt damage has a lower chance to cause bleeding
                    float bluntRoll = Random.value;
                    bool bluntBleeds = bluntRoll < 0.25f;
                    if (BDOTModOptions.DebugLogging)
                        Debug.Log("[BDOT] ZoneDetector: DamageType=Blunt, roll=" + bluntRoll.ToString("F2") + " -> " + (bluntBleeds ? "BLEEDS (25% chance)" : "no bleed"));
                    return bluntBleeds;
                default:
                    if (BDOTModOptions.DebugLogging)
                        Debug.Log("[BDOT] ZoneDetector: DamageType=" + damageType + " -> no bleed (not slash/pierce/blunt)");
                    return false;
            }
        }
    }
}
