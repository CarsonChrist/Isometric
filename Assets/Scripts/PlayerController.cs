using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    Vector3 targetPos;
    Vector3 dashPos;
    Vector3 velocity;
    Vector3 playerLookAt;
    Rigidbody rb;

    public GameObject ammoDisplay;
    public GameObject reloadDisplay;
    public GameObject bullet;
    public float speed;
    public float maxDistance;
    public float dashTimer;
    public float dashTime;
    public float attackSpeed;
    public float reloadTime;
    public int dashPoints;
    public int maxDashes;
    public int maxAmmo;

    float dashCooldown;
    float lastShot;
    int ammo;
    int dashes;
    bool reloading = false;
    bool movementEnabled = true;
    bool shootingEnabled = true;
    bool dashingEnabled = true;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        dashes = maxDashes;
        ammo = maxAmmo;
        lastShot = attackSpeed;
        ammoDisplay.GetComponent<UnityEngine.UI.Text>().text = ammo + "/" + maxAmmo;
    }

    // Update is called once per frame
    void Update()
    {
        // Invisible target on cursor for aiming
        targetPos = MouseToWorld();

        // Dash button press
        if (Input.GetKeyDown("space") && dashes > 0 && dashingEnabled)
        {
            Dash();
            dashes--;
        }

        // Reload on shoot button press if out of ammo
        if (Input.GetMouseButtonDown(0) && ammo == 0 && !reloading)
        {
            reloading = true;
            StartCoroutine(reload(reloadTime));

        }
        // Reload button press
        if (Input.GetKeyDown("r") && ammo < maxAmmo && !reloading)
        {
            reloading = true;
            StartCoroutine(reload(reloadTime));
        }

        // Shoot button press & hold
        lastShot += Time.deltaTime;
        if (Input.GetMouseButton(0) && shootingEnabled)
        {
            movementEnabled = false;
            rb.velocity = Vector3.zero;
            if (ammo > 0)
            {
                if (lastShot >= Time.deltaTime + (1 / attackSpeed))
                {
                    Shoot();
                    ammo--;
                    ammoDisplay.GetComponent<UnityEngine.UI.Text>().text = ammo + "/" + maxAmmo;
                    lastShot = Time.deltaTime;
                }

            }
        }

        // Shoot button release
        if (Input.GetMouseButtonUp(0) && shootingEnabled)
            movementEnabled = true;

    }

    void FixedUpdate()
    {
        // Get input
        velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * speed;

        // 'Rotate' velocity Vector3 45 degrees clockwise
        velocity = Quaternion.Euler(0, 45, 0) * velocity;

        // Store movement direction in case of dash
        playerLookAt = gameObject.transform.position;
        playerLookAt.x += velocity.x;
        playerLookAt.z += velocity.z;

        // Move rigidbody     
        //rb.MovePosition(transform.position + (velocity) * Time.fixedDeltaTime);
        if (movementEnabled)
            rb.velocity = (velocity);

        // Dash cooldown
        if (dashes < maxDashes)
        {
            dashCooldown += Time.fixedDeltaTime;
            if (dashCooldown >= dashTimer)
            {
                dashCooldown = 0;
                dashes = maxDashes;
            }
        }
    }
    // Translate mouse position on screen to a Vector3 in the scene
    Vector3 MouseToWorld()
    {
        // This was from unity forums
        // https://answers.unity.com/questions/1426353/best-way-of-accurately-getting-mouse-position-in-w.html
        Plane plane = new Plane(Vector3.up, transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float dist;
        if (plane.Raycast(ray, out dist))
        {
            return ray.GetPoint(dist);
        }
        else
        {
            return Vector3.zero;
        }
    }
    // Create a bullet prefab that inherits player position and direction
    void Shoot()
    {
        Instantiate(bullet, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 0.75f, gameObject.transform.position.z), Quaternion.LookRotation(targetPos - transform.position));
    }
    // raycast a path of points to dash through then execute them in a coroutine
    void Dash()
    {

        // set dashPos to maxDistance
        dashPos = Vector3.MoveTowards(transform.position, playerLookAt, maxDistance);

        // if dash hits a wall, dash to collision with wall instead as to not go past
        if (Physics.Raycast(transform.position, (dashPos - transform.position), out RaycastHit boundaryHit, Vector3.Distance(dashPos, transform.position), 1 << LayerMask.NameToLayer("Boundary")))
        {
            dashPos = boundaryHit.point;
        }

        StartCoroutine(dashLerp(dashPoints));

    }
    // Move player across given dash points
    IEnumerator dashLerp(int dashPoints)
    {

        Vector3 tempDashPos = dashPos;
        Vector3 midPos;

        for (int i = 0; i < dashPoints; i++)
        {
            midPos = rb.position;
            yield return new WaitForSecondsRealtime(dashTime / dashPoints);
            midPos = Vector3.MoveTowards(midPos, tempDashPos, Vector3.Distance(tempDashPos, midPos) / (dashPoints - i));
            rb.MovePosition(midPos);
        }

    }
    // Reload ammo and update screen text while disabling movement
    IEnumerator reload(float reloadTime)
    {
        movementEnabled = false;
        rb.velocity = Vector3.zero;
        shootingEnabled = false;
        dashingEnabled = false;

        reloadDisplay.GetComponent<UnityEngine.UI.Text>().text = "Reloading.";
        yield return new WaitForSeconds(reloadTime / 3f);
        reloadDisplay.GetComponent<UnityEngine.UI.Text>().text = "Reloading..";
        yield return new WaitForSeconds(reloadTime / 3f);
        reloadDisplay.GetComponent<UnityEngine.UI.Text>().text = "Reloading...";
        yield return new WaitForSeconds(reloadTime / 3f);
        reloadDisplay.GetComponent<UnityEngine.UI.Text>().text = "";


        ammo = maxAmmo;
        ammoDisplay.GetComponent<UnityEngine.UI.Text>().text = ammo + "/" + maxAmmo;

        movementEnabled = true;
        shootingEnabled = true;
        dashingEnabled = true;
        reloading = false;
    }
}
