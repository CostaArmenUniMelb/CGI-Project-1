using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float roll_speed = 1;
    public float pitch_speed = 1;
    public float yaw_speed = 1;

    public float speed_z = 1;
    public float speed_x = 1;

    void Start () {
    }
	
	void Update () {
        TranslationZ();
        TranslationX();
        Roll();
        Pitch();
        Yaw();
    }

    void TranslationZ()
    {
        int direction = 0;
        if (Input.GetKey(KeyCode.W))
        {
            direction = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            direction = -1;
        }

        this.transform.Translate(0.0f, 0.0f, direction * speed_z, Space.Self);
    }

    void TranslationX()
    {
        int direction = 0;
        if (Input.GetKey(KeyCode.A))
        {
            direction = -1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            direction = 1;
        }

        this.transform.Translate(direction * speed_z, 0.0f, 0.0f, Space.Self);
    }

    void Roll()
    {
        int direction = 0;
        if (Input.GetKey(KeyCode.Q))
        {
            direction = 1;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            direction = -1;
        }

        this.transform.Rotate(0.0f, 0.0f, direction * roll_speed, Space.Self);

    }

    void Pitch()
    {
        this.transform.Rotate(-1 * Input.GetAxis("Mouse Y") * pitch_speed, 0.0f, 0.0f, Space.Self);
    }

    void Yaw()
    {
        this.transform.Rotate(0.0f, Input.GetAxis("Mouse X") * yaw_speed, 0.0f, Space.Self);
    }
}
