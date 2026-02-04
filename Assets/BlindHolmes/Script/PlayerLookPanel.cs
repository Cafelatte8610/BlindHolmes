using UnityEngine;

public class PlayerLookPanel : MonoBehaviour
{
    [SerializeField] GameObject player;
 
    void Update()
    {
        transform.LookAt(player.transform); 
    }
}
