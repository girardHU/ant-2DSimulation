using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{

    public TMP_Text FoodInAnthill;
    public TMP_Text FoodOnGround;

    // int nbFoodSource = 3;
    private int nbFoodOnGround = 350;
    private int nbFoodAtAnthill = 0;
    private int nbAnts = 1;
    // Start is called before the first frame update
    void Start()
    {
        // Generate ants and foods
    }

    // Update is called once per frame
    void Update()
    {
        // Count remaining food
        nbFoodOnGround = GameObject.FindGameObjectsWithTag("FoodOnGround").Length;

        // Update UI
        FoodOnGround.text = $"Food Available: {nbFoodOnGround}";
        FoodInAnthill.text = $"Food Gathered: {nbFoodAtAnthill}";
    }

    public void AddFoodToAnthill(int num)
    {
        nbFoodAtAnthill += num;
    }
}
