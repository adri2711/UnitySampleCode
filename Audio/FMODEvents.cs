using FMODUnity;
using UnityEngine;

public class FMODEvents : Singleton<FMODEvents>
{
    [field: Header("Snapshots")]

    [field: Header("Music")]
    [field: SerializeField] public EventReference music { get; private set; }

    [field: Header("Hook SFX")]
    [field: SerializeField] public EventReference hookThrow { get; private set; }
    [field: SerializeField] public EventReference charge { get; private set; }
    [field: SerializeField] public EventReference dash { get; private set; }
    [field: SerializeField] public EventReference enemyAttack { get; private set; }
    [field: SerializeField] public EventReference enemyDamage { get; private set; }
    [field: SerializeField] public EventReference whipAttack { get; private set; }
    [field: SerializeField] public EventReference hammerAttack { get; private set; }
    [field: SerializeField] public EventReference wandAttack { get; private set; }
    [field: SerializeField] public EventReference openSwitchMode { get; private set; }
    [field: SerializeField] public EventReference swap { get; private set; }
    [field: SerializeField] public EventReference catDamage { get; private set; }
    [field: SerializeField] public EventReference land { get; private set; }
    [field: SerializeField] public EventReference playerFootsteps { get; private set; }
}
