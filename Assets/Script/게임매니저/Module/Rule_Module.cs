using UnityEngine;

public class Rule_Module : MonoBehaviour
{
    GameManager_Scene core;
    public int cityDestroyThreshold;
    public int citiesDestroyed;
    public bool isGameOver;
    public void Init(GameManager_Scene gameManager)
    {
        core = gameManager;
        G_Excutor.Subscribe("CityDestroyed", OnCityDestroyed);

    }

    public void ResetState()
    {
        citiesDestroyed = 0;
        isGameOver = false;
    }


    void OnCityDestroyed()
    {
        if (isGameOver) return;

        citiesDestroyed++;
        if (citiesDestroyed >= cityDestroyThreshold)
        {
            isGameOver = true;
            core.Request_RoundLose();
        }
    }


}
