using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ButtonListControl : MonoBehaviour
{
    [SerializeField]
    private GameObject buttonTemplate;

    List<GameObject> buttons = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        string usersDir = Application.persistentDataPath + "/Users";

        if (!Directory.Exists(usersDir))
        {
            Directory.CreateDirectory(usersDir);
        }

        GenerateList(usersDir);
    }

    public void GenerateList(string usersDir)
    {
        var info = new DirectoryInfo(usersDir);
        var fileInfo = info.GetFiles();

        foreach (FileInfo file in fileInfo)
        {
            if (!file.Name.Contains("combinedReadings"))
            {
                var fName = file.Name;
                fName = fName.Replace(".txt", "");

                GameObject btn = Instantiate<GameObject>(buttonTemplate);
                btn.SetActive(true);

                btn.GetComponent<ButtonListButton>().SetText(fName);

                btn.transform.SetParent(buttonTemplate.transform.parent, false);

                buttons.Add(btn);
            }
        }
    }

    public void RefreshList(string usersDir)
    {
        if (buttons.Count > 0)
        {
            foreach (GameObject btn in buttons)
            {
                Destroy(btn.gameObject);
            }
        }

        GenerateList(usersDir);
    }
}
