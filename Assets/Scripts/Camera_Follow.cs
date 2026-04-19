using UnityEngine;

public class Camera_Follow : MonoBehaviour {

    public Transform Target;

    private GameObject Game_Controller;
    private bool Game_Over = false;

    private float Time_ToDown = 0;

    // Screen tint — intensifies with height
    private Color startColor;
    private Color dangerColor = new Color(0.4f, 0.1f, 0.15f); // Dark red tint

    // Use this for initialization
    void Start()
    {
        Game_Controller = GameObject.Find("Game_Controller");
        startColor = Camera.main.backgroundColor;

        // Auto-find the spawned Doodler if Target is not set
        if (Target == null)
        {
            GameObject doodler = GameObject.Find("Doodler");
            if (doodler != null)
                Target = doodler.transform;
        }
    }

    void Update()
    {
        if (Game_Controller == null) return;
        Game_Controller gc = Game_Controller.GetComponent<Game_Controller>();
        if (gc != null) Game_Over = gc.Get_GameOver();
    }

    void FixedUpdate()
    {
        // Move camera to down if game over
        if (Game_Over)
        {
            if(Time.time < Time_ToDown + 4f)
                transform.position -= new Vector3(0, 1f, 0);
            else
            {
                // Delete player and all objects
                GameObject Player = GameObject.FindGameObjectWithTag("Player");
                GameObject[] Objects = GameObject.FindGameObjectsWithTag("Object");

                Destroy(Player);
                foreach (GameObject Obj in Objects)
                    Destroy(Obj);
            }
        }
    }

	void LateUpdate () 
    {
        if(!Game_Over && Target != null)
        {
            // Camera only moves up, never down (follows the highest point)
            float targetY = Target.position.y;
            if (targetY > transform.position.y)
            {
                Vector3 New_Pos = new Vector3(transform.position.x, targetY, transform.position.z);
                transform.position = Vector3.Lerp(transform.position, New_Pos, Time.deltaTime * 3f);
            }

            Time_ToDown = Time.time;
        }

        // Screen tint — gradually shifts to danger color as player climbs
        if (Target != null && !Game_Over)
        {
            float heightFactor = Mathf.Clamp(Target.position.y / 300f, 0f, 1f);
            Camera.main.backgroundColor = Color.Lerp(startColor, dangerColor, heightFactor);
        }
	}

}
