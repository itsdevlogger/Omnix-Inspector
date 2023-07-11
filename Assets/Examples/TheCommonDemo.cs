using UnityEngine;
using UnityEngine.Events;

namespace Omnix.Demos
{
    // A class with a number of variables with different builtin types and nested classes with multiple levels of nesting
    public class TheCommonDemo : MonoBehaviour
    {
        [SerializeField] private Sprite avatar;
        [SerializeField] private bool isActive;
        [SerializeField] private string myName;
        [SerializeField] private float health;
        [SerializeField] private int level;
        [SerializeField] private int power;

        [Header("Ground Movement Info")]
        [SerializeField] private float currentMoveSpeed;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float walkSpeed;
        [SerializeField] private float runSpeed;

        [Header("Air Movement Info")]
        [SerializeField] private float isInAir;
        [SerializeField] private float maxJumpHeight;
        [SerializeField] private float gravityMultiplier;

        [Header("Events Info")]
        #if UNITY_EDITOR
        [SerializeField] private bool hasLaunchCallback;
        [SerializeField] private bool hasSpawnCallback;
        [SerializeField] private bool hasLandCallback;
        [SerializeField] private bool hasDiedCallback;
        #endif

        [SerializeField] private UnityEvent onSpawnCallback;
        [SerializeField] private UnityEvent onLaunchCallback;
        [SerializeField] private UnityEvent onLandCallback;
        [SerializeField] private UnityEvent onDiedCallback;

        private void SpawnCallback() => Debug.Log($"Spawned enemy {myName}");
        private void LaunchCallback() => Debug.Log($"Launch {myName}");
        private void LandCallback() => Debug.Log($"Land {myName}");
        private void DiedCallback() => Debug.Log($"Died {myName}");
    }

}