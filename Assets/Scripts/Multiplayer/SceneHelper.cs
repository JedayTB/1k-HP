using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHelper : MonoBehaviour
{
   public static bool IsInMPLobby()
   {
      return SceneManager.GetActiveScene().name == "MP_Lobby";
   }
   
   public static bool IsInMPLevel()
   {
      return SceneManager.GetActiveScene().name == "MP_CityLevel";
   }
}
