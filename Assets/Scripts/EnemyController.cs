using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float maxHealth;
    public float respawnTimer;
    public float speed;
    float health;
    float lastDeath;
    bool alive = true;
    public GameObject[] route;
    int routeStatus;

    // Start is called before the first frame update
    void Start()
    {
        routeStatus = 0;
        lastDeath = respawnTimer;
        health = maxHealth;
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(200 / 255f, 170 / 255f, 0);
        StartCoroutine(moveOnTrack());
    }

    // Update is called once per frame
    void Update()
    {
        // Kill enemy when reaching zero health
        if (health <= 0 && alive == true)
        {
            death();
            lastDeath = 0;
        }

        // Respawn enemy after respawn timer
        lastDeath += Time.deltaTime;
        if (lastDeath >= respawnTimer && alive == false)
        {
            respawn();
            StartCoroutine(moveOnTrack());

        }
    }
    // Change enemy material color and transparency, disable collider
    public void death()
    {
        alive = false;
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(200 / 255f, 170 / 255f, 0, 0.5f);
        gameObject.GetComponent<CapsuleCollider>().enabled = false;

    }
    // Change enemy color and transparency, enable collider
    public void respawn()
    {
        alive = true;
        health = maxHealth;
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(200 / 255f, 170 / 255f, 0, 1f);
        gameObject.GetComponent<CapsuleCollider>().enabled = true;

    }
    // Subtract damage taken from health, shift color towards red
    // Called in bullet class
    public void takeDamage(float damage)
    {
        health -= damage;
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(200 / 255f, (health / maxHealth) * 150 / 255f, 0, 1f);
    }
    // Move enemy along a list of points in the scene
    IEnumerator moveOnTrack()
    {
        while (alive)
        {
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, new Vector3(route[routeStatus].transform.position.x, gameObject.transform.position.y, route[routeStatus].transform.position.z), speed * Time.deltaTime);
            yield return null;
            if (gameObject.transform.position == new Vector3(route[routeStatus].transform.position.x, gameObject.transform.position.y, route[routeStatus].transform.position.z))
            {
                if (routeStatus >= route.Length - 1)
                {
                    routeStatus = 0;
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    routeStatus++;
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
    }

}
