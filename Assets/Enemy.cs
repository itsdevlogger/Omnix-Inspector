using UnityEngine;
using UnityEngine.Events;

// A class with a number of variables with different builtin types and nested classes with multiple levels of nesting
public class Enemy : MonoBehaviour
{
    [SerializeField] private Sprite avatar;
    [SerializeField] private bool isActive;
    [SerializeField] private string myName;
    [SerializeField] private float health;
    [SerializeField] private int level;

    [Header("Ground Movement Info")]
    [SerializeField] private float isMoving;
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



}
