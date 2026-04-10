using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform_Generator : MonoBehaviour {

    public GameObject Platform_Green;
    public GameObject Platform_Blue;
    public GameObject Platform_White;
    public GameObject Platform_Brown;

    public GameObject Spring;
    public GameObject Trampoline;
    public GameObject Propeller;

    private GameObject Platform;
    private GameObject Random_Object;

    public float Current_Y = 0;
    float Offset;
    Vector3 Top_Left;

    // Enemy spawning
    private float nextEnemyHeight = 30f; // First enemy appears at height 30

    // Coin spawning - every 7 platforms
    private int platformCount = 0;

	// Use this for initialization
	void Start ()
    {
        // Initialize boundary
        Top_Left = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Offset = 1.2f;

        // Spawn a starting platform directly under the doodler's feet
        Vector3 startPlatformPos = new Vector3(0f, -4.0f, 0f);
        Instantiate(Platform_Green, startPlatformPos, Quaternion.identity);

        // Initialize platforms
        Generate_Platform(15);
	}
	
	// Ensure platforms always exist above the camera view
	void Update ()
	{
	    if (!Game_Controller.Game_Started) return;

	    Camera cam = Camera.main;
	    if (cam == null) return;

	    // Always keep platforms generated at least 30 units above the camera top
	    float cameraTop = cam.transform.position.y + cam.orthographicSize;
	    float buffer = 30f;

	    while (Current_Y < cameraTop + buffer)
	    {
	        Generate_Platform(1);
	    }
	}

    public void Generate_Platform(int Num)
    {
        for (int i = 0; i < Num; i++)
        {
            // Calculate platform x, y
            float Dist_X = Random.Range(Top_Left.x + Offset, -Top_Left.x - Offset);

            // Increasing difficulty: platforms get farther apart as you climb
            float heightFactor = Mathf.Clamp(Current_Y / 200f, 0f, 1f);
            float minDist = Mathf.Lerp(1.2f, 1.8f, heightFactor);
            float maxDist = Mathf.Lerp(3f, 4.5f, heightFactor);
            float Dist_Y = Random.Range(minDist, maxDist);

            // Create brown platform random with 1/8 probability
            int Rand_BrownPlatform = Random.Range(1, 8);

            if (Rand_BrownPlatform == 1)
            {
                float Brown_DistX = Random.Range(Top_Left.x + Offset, -Top_Left.x - Offset);
                float Brown_DistY = Random.Range(Current_Y + 1, Current_Y + Dist_Y - 1);
                Vector3 BrownPlatform_Pos = new Vector3(Brown_DistX, Brown_DistY, 0);

                Instantiate(Platform_Brown, BrownPlatform_Pos, Quaternion.identity);
            }

            // Calculate difficulty scale based on current height
            float difficultyScale = Mathf.Clamp(Current_Y / 100f, 0f, 1f);

            // Create other platform
            Current_Y += Dist_Y;
            Vector3 Platform_Pos = new Vector3(Dist_X, Current_Y, 0);

            // Progressive difficulty: upper bounds decrease as you get higher
            // Originally random range of 1-10 (exclusive max, so 1-9) -> 1(blue), 2(white), 3-9(green).
            int maxRand = Mathf.FloorToInt(Mathf.Lerp(10f, 4f, difficultyScale));
            int Rand_Platform = Random.Range(1, maxRand);

            if (Rand_Platform == 1) // Create blue platform
                Platform = Instantiate(Platform_Blue, Platform_Pos, Quaternion.identity);
            else if (Rand_Platform == 2) // Create white platform
                Platform = Instantiate(Platform_White, Platform_Pos, Quaternion.identity);
            else // Create green platform
                Platform = Instantiate(Platform_Green, Platform_Pos, Quaternion.identity);

            // Spawn coin every 2 platforms (only on green/blue, not white/brown)
            platformCount++;
            if (platformCount % 2 == 0 && Rand_Platform != 2 && Rand_BrownPlatform != 1)
            {
                float coinY = Platform_Pos.y + 0.7f;
                GameObject coin = new GameObject("Coin");
                coin.transform.position = new Vector3(Platform_Pos.x, coinY, 0);
                coin.AddComponent<Coin>();
            }

            // Spawn enemy near platform (gets more frequent with height)
            if (Current_Y >= nextEnemyHeight)
            {
                float enemyX = Random.Range(Top_Left.x + Offset, -Top_Left.x - Offset);
                float enemyY = Current_Y + Random.Range(0.5f, 1.5f);
                GameObject enemy = new GameObject("Enemy");
                enemy.transform.position = new Vector3(enemyX, enemyY, 0);
                enemy.AddComponent<Enemy>();

                // Next enemy spawns sooner as height increases
                float enemyHeightFactor = Mathf.Clamp(Current_Y / 200f, 0f, 1f);
                float spacing = Mathf.Lerp(40f, 15f, enemyHeightFactor);
                nextEnemyHeight = Current_Y + spacing;
            }

            if (Rand_Platform != 2)
            {
                // Create random objects; like spring, trampoline and etc...
                int Rand_Object = Random.Range(1, 40);

                if (Rand_Object == 4) // Create spring
                {
                    Vector3 Spring_Pos = new Vector3(Platform_Pos.x + 0.5f, Platform_Pos.y + 0.27f, 0);
                    Random_Object = Instantiate(Spring, Spring_Pos, Quaternion.identity);
                    
                    // Set parent to object
                    Random_Object.transform.parent = Platform.transform;
                }
                else if (Rand_Object == 7) // Create trampoline
                {
                    Vector3 Trampoline_Pos = new Vector3(Platform_Pos.x + 0.13f, Platform_Pos.y + 0.25f, 0);
                    Random_Object = Instantiate(Trampoline, Trampoline_Pos, Quaternion.identity);

                    // Set parent to object
                    Random_Object.transform.parent = Platform.transform;
                }
                else if (Rand_Object == 15) // Create propeller
                {
                    Vector3 Propeller_Pos = new Vector3(Platform_Pos.x + 0.13f, Platform_Pos.y + 0.15f, 0);
                    Random_Object = Instantiate(Propeller, Propeller_Pos, Quaternion.identity);

                    // Set parent to object
                    Random_Object.transform.parent = Platform.transform;
                }
            }
        }
    }
}
