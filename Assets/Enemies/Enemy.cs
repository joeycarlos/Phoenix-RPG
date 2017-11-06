﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class Enemy : MonoBehaviour, IDamageable {

    [SerializeField] float maxHealthPoints = 100f;
    [SerializeField] float chaseRadius = 10f;

    [SerializeField] float attackRadius = 3f;
    [SerializeField] float damagePerShot = 9f;

    [SerializeField] GameObject projectileToUse;
    [SerializeField] GameObject projectileSocket;

    float currentHealthPoints = 100f;
    AICharacterControl aiCharacterControl = null;
    GameObject player = null;

    private void Start()
    {
        aiCharacterControl = GetComponent<AICharacterControl>();
        player = GameObject.FindGameObjectWithTag("Player");

    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

        if (distanceToPlayer <= chaseRadius)
        {
            aiCharacterControl.SetTarget(player.transform);
        } else
        {
            aiCharacterControl.SetTarget(transform);
        }

        if (distanceToPlayer <= attackRadius)
        {
            SpawnProjectile();
        }
    }

    public float healthAsPercentage
    {
        get
        {
            return currentHealthPoints / (float) maxHealthPoints;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealthPoints = Mathf.Clamp(currentHealthPoints - damage, 0f, maxHealthPoints);
    }

    public void SpawnProjectile()
    {
        GameObject newProjectile = Instantiate(projectileToUse, projectileSocket.transform.position, Quaternion.identity);
        var projectileComponent = newProjectile.GetComponent<Projectile>();
        projectileComponent.damageCaused = damagePerShot;

        Vector3 unitVectorToPlayer = (player.transform.position - projectileSocket.transform.position).normalized;
        float projectileSpeed = projectileComponent.projectileSpeed;
        newProjectile.GetComponent<Rigidbody>().velocity = unitVectorToPlayer * projectileSpeed;
    }

    void OnDrawGizmos()
    {
        // Draw attack sphere
        Gizmos.color = new Color(255f, 0f, 0, .5f);
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        // Draw move sphere
        Gizmos.color = new Color(0f, 0f, 255f, .5f);
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
    }
}
