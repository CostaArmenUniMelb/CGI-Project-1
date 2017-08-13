using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* The cameraController controls the translational
 * and rotational movement of the camera via mouse
 * and key inputs. The speed of all movements are configurable
 * via unity. All movements are relative to the local coordinates
 * of the camera
 */
public class CameraController : MonoBehaviour {

    public float roll_speed = 1;
    public float pitch_speed = 1;
    public float yaw_speed = 1;

    public float speed_z = 0.1f;
    public float speed_x = 0.1f;
	
	void Update () {
        TranslationZ();
        TranslationX();
        Roll();
        Pitch();
        Yaw();
    }

    /* A direction is determined by the key inputs (W,S)
     * The magnitude of translation is the speed (speed_z)
     */
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

    /* A direction is determined by the key inputs (A,D)
     * The magnitude of translation is the speed (speed_x)
     */
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

        this.transform.Translate(direction * speed_x, 0.0f, 0.0f, Space.Self);
    }

    /* A direction is determined by the key inputs (Q,E)
     * The magnitude of rotation is the roll speed (roll_speed)
     */
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

    /* A direction is determined by the mouse input along the y axis
     * The magnitude of rotation is the pitch speed (pitch_speed)
     */
    void Pitch()
    {
        this.transform.Rotate(-1 * Input.GetAxis("Mouse Y") * pitch_speed, 0.0f, 0.0f, Space.Self);
    }

    /* A direction is determined by the mouse input along the x axis
     * The magnitude of rotation is the yaw speed (yaw_speed)
     */
    void Yaw()
    {
        this.transform.Rotate(0.0f, Input.GetAxis("Mouse X") * yaw_speed, 0.0f, Space.Self);
    }
}
