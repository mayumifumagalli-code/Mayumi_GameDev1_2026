using UnityEngine;

public class Tests : MonoBehaviour
{
    public int MyInt;
    public float MyFloat;
    public string MyString;
    public bool MyBool;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    { 
        MyInt = 1;
        MyFloat = 4.56f;
        MyString = "WTFFF";
        MyBool = true;
    }

    // Update is called once per frame
    void Update()
    {
        MyInt = MyInt + 3;
        MyFloat = MyFloat + 3.4f;
        MyString = MyString + " wow";
        MyBool = !MyBool;
    }
}
