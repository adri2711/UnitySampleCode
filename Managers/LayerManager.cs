using UnityEngine;

namespace Managers
{
    public class LayerManager
    {
        private enum Layers
        {
            DEFAULT = 0,
            GROUND_LAYER = 6,
            INTERACTABLE_LAYER,
            ALLY_LAYER,
            ENEMY_LAYER,
            ALLY_ATTACK_ZONE,
            ENEMY_ATTACK_ZONE
        };

        public static int GetInteractableLayer()
        {
            return (int)Layers.INTERACTABLE_LAYER;
        }

        public static int GetAllyLayer()
        {
            return (int)Layers.ALLY_LAYER;
        }

        public static int GetEnemyLayer()
        {
            return (int)Layers.ENEMY_LAYER;
        }

        public static int GetGroundLayer()
        {
            return (int)Layers.GROUND_LAYER;
        }

        public static int GetAllyAttackZoneLayer()
        {
            return (int)Layers.ALLY_ATTACK_ZONE;
        }

        public static int GetEnemyAttackZoneLayer()
        {
            return (int)Layers.ENEMY_ATTACK_ZONE;
        }

        public static int GetRaycastLayers()
        {
            return 1 << (int)Layers.INTERACTABLE_LAYER
                 | 1 << (int)Layers.GROUND_LAYER
                 | 1 << (int)Layers.ALLY_LAYER
                 | 1 << (int)Layers.ENEMY_LAYER
                 | 1 << (int)Layers.DEFAULT;
        }

        public static int GetRaycastLayersWithoutAlly()
        {
            return 1 << (int)Layers.INTERACTABLE_LAYER
                 | 1 << (int)Layers.GROUND_LAYER
                 | 1 << (int)Layers.ENEMY_LAYER
                 | 1 << (int)Layers.DEFAULT;
        }
    }
}