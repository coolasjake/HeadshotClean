using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityChanger : MonoBehaviour
{
    public Gravity.GravityAbility ability;
    public Gravity.AbilityStatus status;

    public void ChangeAbility()
    {
        print("Setting " + ability.ToString() + " to " + status.ToString());
        Movement.ThePlayer.GetComponent<Gravity>().SetAbilityStatus(ability, status);
    }
}
