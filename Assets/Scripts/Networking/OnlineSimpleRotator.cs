﻿using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.CrossPlatformInput;

public class OnlineSimpleRotator : NetworkBehaviour
{
    public Vector2 rotationRange = new Vector3(180, 361);
    public float rotationSpeed = 3;
    public float dampingTime = 0;
    public bool autoZeroVerticalOnMobile = true;
    public bool autoZeroHorizontalOnMobile = false;
    public bool relative = true;


    private Vector3 m_TargetAngles;
    private Vector3 m_FollowAngles;
    private Vector3 m_FollowVelocity;
    private Quaternion m_OriginalRotation;


    private void Start()
    {
        m_OriginalRotation = transform.localRotation;
    }


    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        // we make initial calculations from the original local rotation
        transform.localRotation = m_OriginalRotation;

        // read input from mouse or mobile controls
        float inputH;
        float inputV;
        if (relative)
        {
            inputH = CrossPlatformInputManager.GetAxis("Mouse X");
            inputV = CrossPlatformInputManager.GetAxis("Mouse Y");

            // wrap values to avoid springing quickly the wrong way from positive to negative
            if (m_TargetAngles.y > 180)
            {
                m_TargetAngles.y -= 360;
                m_FollowAngles.y -= 360;
            }
            if (m_TargetAngles.x > 180)
            {
                m_TargetAngles.x -= 360;
                m_FollowAngles.x -= 360;
            }
            if (m_TargetAngles.y < -180)
            {
                m_TargetAngles.y += 360;
                m_FollowAngles.y += 360;
            }
            if (m_TargetAngles.x < -180)
            {
                m_TargetAngles.x += 360;
                m_FollowAngles.x += 360;
            }

#if MOBILE_INPUT
            // on mobile, sometimes we want input mapped directly to tilt value,
            // so it springs back automatically when the look input is released.
			if (autoZeroHorizontalOnMobile) {
				m_TargetAngles.y = Mathf.Lerp (-rotationRange.y * 0.5f, rotationRange.y * 0.5f, inputH * .5f + .5f);
			} else {
				m_TargetAngles.y += inputH * rotationSpeed;
			}
			if (autoZeroVerticalOnMobile) {
				m_TargetAngles.x = Mathf.Lerp (-rotationRange.x * 0.5f, rotationRange.x * 0.5f, inputV * .5f + .5f);
			} else {
				m_TargetAngles.x += inputV * rotationSpeed;
			}
#else
            // with mouse input, we have direct control with no springback required.
            m_TargetAngles.y += inputH * rotationSpeed;
            m_TargetAngles.x += inputV * rotationSpeed;
#endif

            // clamp values to allowed range
            m_TargetAngles.y = Mathf.Clamp(m_TargetAngles.y, -rotationRange.y * 0.5f, rotationRange.y * 0.5f);
            m_TargetAngles.x = Mathf.Clamp(m_TargetAngles.x, -rotationRange.x * 0.5f, rotationRange.x * 0.5f);
        }
        else
        {
            inputH = Input.mousePosition.x;
            inputV = Input.mousePosition.y;

            // set values to allowed range
            m_TargetAngles.y = Mathf.Lerp(-rotationRange.y * 0.5f, rotationRange.y * 0.5f, inputH / Screen.width);
            m_TargetAngles.x = Mathf.Lerp(-rotationRange.x * 0.5f, rotationRange.x * 0.5f, inputV / Screen.height);
        }

        // smoothly interpolate current values to target angles
        m_FollowAngles = Vector3.SmoothDamp(m_FollowAngles, m_TargetAngles, ref m_FollowVelocity, dampingTime);

        // update the actual gameobject's rotation
        transform.localRotation = m_OriginalRotation * Quaternion.Euler(-m_FollowAngles.x, m_FollowAngles.y, 0);
    }
}
