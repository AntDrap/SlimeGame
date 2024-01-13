using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
    public string inputUserName;
    public string inputPassword;
    public string inputEmail;

    string CreateUserURL = "http://localhost/prevision/insertUser.php";

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CreateUser());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator CreateUser()
    {
        WWWForm form = new WWWForm();
        form.AddField("test", "test");

        UnityWebRequest www = UnityWebRequest.Post(CreateUserURL, form);

        yield return www.SendWebRequest();

        if (www.error == null)
        {
            // you can place code here for handle a succesful post
        }
        else
        {
            // what to do on error
        }

    }
}
