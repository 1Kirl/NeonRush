using UnityEngine;
using UnityEngine.SceneManagement;
using Shared.Protocol;
using System.Runtime.ExceptionServices;
using System.Collections.Generic;
public class MainSceneManager : MonoBehaviour
{
    public string sceneName;
    [SerializeField]private List<string> multiMaps = new();
    private string changeToThisScene = "";
    public void ChangeScene()
    {
        SceneManager.LoadScene(sceneName);
    }
    public string ChangeToMulti(MapType mapType)
    {
        switch (mapType)
        {
            case MapType.first:
                changeToThisScene = multiMaps[0];
                break;
            case MapType.second:
                changeToThisScene = multiMaps[1];
                break;
            case MapType.third:
                changeToThisScene = multiMaps[2];
                break;
            default:
                changeToThisScene = multiMaps[0];
                break;
        }
        SceneManager.LoadScene(changeToThisScene);
        return changeToThisScene;
    }
}