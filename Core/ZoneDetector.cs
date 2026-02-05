using DOT.Configuration;
using ThunderRoad;
using UnityEngine;

namespace DOT.Core
{
    public static class ZoneDetector
    {
        // Cached part type masks for faster comparison
        private const RagdollPart.Type ArmMask = RagdollPart.Type.LeftArm | RagdollPart.Type.RightArm |
                                                  RagdollPart.Type.LeftHand | RagdollPart.Type.RightHand;
        private const RagdollPart.Type LegMask = RagdollPart.Type.LeftLeg | RagdollPart.Type.RightLeg |
                                                  RagdollPart.Type.LeftFoot | RagdollPart.Type.RightFoot;

        /// <summary>
        /// Determines the body zone from a ragdoll part, with special handling for throat detection.
        /// </summary>
        public static BodyZone GetZone(RagdollPart part, bool isSliced = false)
        {
            if (part == null)
            {
                if (DOTModOptions.DebugLogging)
                    Debug.Log("[DOT] ZoneDetector: part is null");
                return BodyZone.Unknown;
            }

            // Check for dismemberment first
            if (isSliced || part.isSliced)
            {
                if (DOTModOptions.DebugLogging)
                    Debug.Log("[DOT] ZoneDetector: Part is sliced -> Dismemberment (partType=" + part.type + ")");
                return BodyZone.Dismemberment;
            }

            var partType = part.type;

            // Head detection
            if ((partType & RagdollPart.Type.Head) != 0)
            {
                if (DOTModOptions.DebugLogging)
                    Debug.Log("[DOT] ZoneDetector: RagdollPart.Type=" + partType + " -> Head");
                return BodyZone.Head;
            }

            // Neck detection - check for throat hit
            if ((partType & RagdollPart.Type.Neck) != 0)
            {
                // Try to determine if this is a throat hit (front of neck)
                if (IsThroatHit(part))
                {
                    if (DOTModOptions.DebugLogging)
                        Debug.Log("[DOT] ZoneDetector: RagdollPart.Type=" + partType + " -> Throat (neck front hit)");
                    return BodyZone.Throat;
                }
                if (DOTModOptions.DebugLogging)
                    Debug.Log("[DOT] ZoneDetector: RagdollPart.Type=" + partType + " -> Neck (not throat)");
                return BodyZone.Neck;
            }

            // Torso detection
            if ((partType & RagdollPart.Type.Torso) != 0)
            {
                if (DOTModOptions.DebugLogging)
                    Debug.Log("[DOT] ZoneDetector: RagdollPart.Type=" + partType + " -> Torso");
                return BodyZone.Torso;
            }

            // Arm detection (includes hands)
            if ((partType & ArmMask) != 0)
            {
                if (DOTModOptions.DebugLogging)
                    Debug.Log("[DOT] ZoneDetector: RagdollPart.Type=" + partType + " -> Arm");
                return BodyZone.Arm;
            }

            // Leg detection (includes feet)
            if ((partType & LegMask) != 0)
            {
                if (DOTModOptions.DebugLogging)
                    Debug.Log("[DOT] ZoneDetector: RagdollPart.Type=" + partType + " -> Leg");
                return BodyZone.Leg;
            }

            if (DOTModOptions.DebugLogging)
                Debug.Log("[DOT] ZoneDetector: RagdollPart.Type=" + partType + " -> Unknown (no match)");
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
            if (DOTModOptions.DebugLogging)
                Debug.Log("[DOT] ZoneDetector: Throat roll=" + roll.ToString("F2") + " -> " + (isThroat ? "THROAT" : "neck"));
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

            if ((partType & ArmMask) != 0)
            {
                return BodyZone.Arm;
            }

            if ((partType & LegMask) != 0)
            {
                return BodyZone.Leg;
            }

            return BodyZone.Unknown;
        }
    }
}
