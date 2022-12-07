using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerHealth : MonoBehaviour
{

    public List<HealthChunk> healthChunks = new List<HealthChunk>();
    private int currentChunk = 0;

    public DeathOutcome outcome = DeathOutcome.gameOver;
    public enum DeathOutcome
    {
        respawn,
        loadCheckpoint,
        restart,
        gameOver,
    }

    public RespawnPoint spawnPoint;
    private Vector3 startingPos;
    public string gameOverScene = "GameOver";
    public string restartScene = "Restart";

    public UnityEvent OnDeath = new UnityEvent();

    public float deathAnimationTime = 3f;

    private PlayerMovement movement;

    private float invulnerabilityEndTime = 0;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        startingPos = transform.position;
    }

    private void Kill(string killerName)
    {
        DeathAnimationThenOutcome();
    }

    private IEnumerator DeathAnimationThenOutcome()
    {
        OnDeath.Invoke();
        WaitForSeconds wait = new WaitForSeconds(invulnerabilityEndTime - Time.time);
        yield return wait;

        switch (outcome)
        {
            case DeathOutcome.respawn:
                if (spawnPoint != null)
                    movement.Teleport(spawnPoint.TeleportLocation, spawnPoint.Rotation.x, spawnPoint.Rotation.y, spawnPoint.DeleteVelocity);
                else
                    movement.Teleport(startingPos);
                break;
            case DeathOutcome.loadCheckpoint:
                //TODO: checkpoint system
                break;
            case DeathOutcome.gameOver:
                SceneManager.LoadScene(gameOverScene);
                break;
            case DeathOutcome.restart:
                SceneManager.LoadScene(restartScene);
                break;
        }
    }

    private void Hit(float damage, string attackerName)
    {
        while (damage > 0 && currentChunk < healthChunks.Count)
        {
            if (Time.time < invulnerabilityEndTime)
            {
                return;
            }

            if (healthChunks[currentChunk].Hit(ref damage))
            {
                if (currentChunk == healthChunks.Count - 1)
                    Kill(attackerName);
                else
                {
                    float newInvulDur = Mathf.Max(invulnerabilityEndTime, healthChunks[currentChunk].invulnerabilityDuration);
                    invulnerabilityEndTime = Time.time + newInvulDur;
                    ShowInvulnerability();
                    currentChunk += 1;
                }
            }
        }
    }


    private Coroutine _finishInvulCoR;
    private void ShowInvulnerability()
    {
        //Show UI etc

        if (_finishInvulCoR != null)
            StopCoroutine(_finishInvulCoR);
        _finishInvulCoR = StartCoroutine(FinishShowingInvul());
    }

    private IEnumerator FinishShowingInvul()
    {
        WaitForSeconds wait = new WaitForSeconds(invulnerabilityEndTime - Time.time);
        yield return wait;

        //Hide ui etc
    }

    [System.Serializable]
    public class HealthChunk
    {
        public string name = "Unnamed Chunk";
        public float maxHealth = 50f;
        public float Health { get; private set; } = 0;
        public Image UIBar;
        public bool overflowDamage = false;
        public float invulnerabilityDuration = 1f;
        public UnityEvent breakEffects = new UnityEvent();

        public HealthChunk()
        {
            Health = maxHealth;
        }

        /// <summary>
        /// Deal damage to this chunk of health, then return true if it was destroyed so that invulnerability can be applied if necessary,
        /// and calculate any extra damage that needs to be overflowed.
        /// </summary>
        /// <param name="damage"> Damage to this chunk. </param>
        /// <param name="extraDamage"> Remaining damage after this chunk is destroyed. Automatically 0 if overflowDamage is off. </param>
        /// <returns> True if the chunk was destroyed. </returns>
        public bool Hit(ref float damage)
        {
            Health -= damage;
            UpdateUI();
            if (Health <= 0)
            {
                if (overflowDamage)
                    damage = -Health;
                Health = 0;
                Break();
                return true;
            }
            return false;
        }

        public void UpdateUI()
        {
            if (UIBar != null)
                UIBar.fillAmount = Health / maxHealth;
        }

        public void Break()
        {
            breakEffects.Invoke();
        }
    }
}