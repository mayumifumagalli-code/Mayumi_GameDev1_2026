using UnityEngine;

public class CubeControlller : MonoBehaviour
{
    //public float CubeScaleX;
    //public float CubeScaleY;
    //public float CubeScaleZ;

    //This is called a composite variable
    public Vector3 NewCubeValue = new Vector3(0.3f, 0.5f, 0.7f);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //transform.localScale = transform.localScale + NewCubeValue;
        //transform.localPosition = transform.localPosition + NewCubeValue;
    }

    public void SetCUbeToLarge()
    {
        transform.localScale = new Vector3(8f, 10f, 12f);
    }

    public void SetCubeScale(float scale)
    {
        transform.localScale = transform.localScale + new Vector3(scale, scale, scale);
    }
}
