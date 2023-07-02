using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    // Fields
    [SerializeField] private int loadScene = -1;
    [SerializeField] private Transform spawn;
    private GamerController _gamer;
    
    // Properties
    public Transform Spawn => spawn;
    
    public void OnPlayerTriggered(GamerController gamer)
    {
        _gamer = gamer;
        StartCoroutine(SwitchScene());
    }

    private IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject); // Don't destroy portal
        yield return SceneManager.LoadSceneAsync(loadScene);
        Portal destination = FindObjectsOfType<Portal>().First(x => x != this); // Find portal which is not this one
        _gamer.Character.SetPositionAndSnapToTile(destination.spawn.position);;
        Destroy(gameObject); // After executing code, portal can be destroyed
    }
}
