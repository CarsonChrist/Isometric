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
    public GameObject reloadDisplayBg;
    public GameObject bullet;
    public float speed;
    public float maxDistance;
    public float dashTimer;
    public float dashTime;
    public float attackSpeed;
    public float reloadTime;
    public int maxDashes;
    public int maxAmmo;

    float dashCooldown;
    float lastShot;
    int ammo;
    int dashes;
    bool dashInput = false;
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
            dashInput = true;
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
                if (lastShot >= Time.deltaTime + (1 / attackSpeed) && PauseMenu.isPaused == false)
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
        if (movementEnabled)
        {
            rb.velocity = (velocity);
        }
        
        if (dashInput)
        {
            Dash();
            dashes--;
            dashInput = false;
        }

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
    // Raycast a path of points to dash through then execute them in a coroutine
    void Dash()
    {

        // Set dashPos to maxDistance
        dashPos = Vector3.MoveTowards(transform.position, playerLookAt, maxDistance);

        // If dash hits a wall, dash to collision with wall instead as to not go past
        if (Physics.Raycast(transform.position, (dashPos - transform.position), out RaycastHit boundaryHit, Vector3.Distance(dashPos, transform.position), 1 << LayerMask.NameToLayer("Boundary")))
        {
            dashPos = boundaryHit.point;
        }

        StartCoroutine(dashLerp(dashTime));

    }
    // Move player across given dash points
    IEnumerator dashLerp(float dashTime)
    {
        // https://answers.unity.com/questions/1501234/smooth-forward-movement-with-a-coroutine.html
        Vector3 tempDashPos = dashPos;
        Vector3 midPos;
        float elapsedTime = 0;

        while (elapsedTime < dashTime)
        {
            midPos = rb.position;
            midPos = Vector3.Lerp(transform.position, dashPos, (elapsedTime/dashTime));
            rb.MovePosition(midPos);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    
    }
    // Reload ammo and update screen text while disabling movement
    IEnumerator reload(float reloadTime)
    {
        movementEnabled = false;
        rb.velocity = Vector3.zero;
        shootingEnabled = false;
        dashingEnabled = false;

        reloadDisplayBg.GetComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,120);
        reloadDisplay.GetComponent<UnityEngine.UI.Text>().text = "Reloading.";
        yield return new WaitForSeconds(reloadTime / 3f);
        reloadDisplay.GetComponent<UnityEngine.UI.Text>().text = "Reloading..";
        yield return new WaitForSeconds(reloadTime / 3f);
        reloadDisplay.GetComponent<UnityEngine.UI.Text>().text = "Reloading...";
        yield return new WaitForSeconds(reloadTime / 3f);
        reloadDisplayBg.GetComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,0);
        reloadDisplay.GetComponent<UnityEngine.UI.Text>().text = "";

        ammo = maxAmmo;
        ammoDisplay.GetComponent<UnityEngine.UI.Text>().text = ammo + "/" + maxAmmo;

        movementEnabled = true;
        shootingEnabled = true;
        dashingEnabled = true;
        reloading = false;
    }
}
