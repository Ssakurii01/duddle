using UnityEngine;

public class Camera_Follow : MonoBehaviour {

    public Transform Target;

    private GameObject Game_Controller;
    private bool Game_Over = false;

    private float Time_ToDown = 0;

    // Use this for initialization
    void Start()
    {
        Game_Controller = GameObject.Find("Game_Controller");

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
        // On game over: freeze the camera and clean up props after a short delay
        // so the game-over card + sky stay stable instead of rewinding through altitudes.
        if (!Game_Over) return;

        if (Time.time < Time_ToDown + 4f) return;

        GameObject Player = GameObject.FindGameObjectWithTag("Player");
        if (Player != null) Destroy(Player);

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
            Destroy(obj);
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
	}

}
