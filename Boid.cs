using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    public float _zoneRepulsion = 5;
    public float _zoneAlignement = 7;
    public float _zoneAttraction = 50;

    public float _forceRepulsion = 30;
    public float _forceAlignement = 30;
    public float _forceAttraction = 30;

    public Vector3 _target = new Vector3();
    public float _forceTarget = 20;
    public bool _goToTarget = false;

    public Vector3 _landingZone = new Vector3();
    public bool _goToLandingZone = false;
    public bool _isLanded = false;
    public bool _isTakingOff = false;

    public Vector3 _velocity = new Vector3();
    public float _maxSpeed = 20;
    public float _minSpeed = 12;

    public bool _drawGizmos = true;
    public bool _drawLines = true;


    public Vector3 _skyPos;

    public bool _canLand;

    public float _landingDuration = 5;

    private void Update()
    {
         Vector3 sumForces = new Vector3();
         Color colorDebugForce = Color.black;
         float nbForcesApplied = 0;
        
         foreach (Boid otherBoid in BoidManager._sharedInstance.roBoids)
         {
            Vector3 vecToOtherBoid = otherBoid.transform.position - transform.position;
            Vector3 forceToApply = new Vector3();

            if (transform.position.y < 5 && transform.position.y > 1.5 && !_goToLandingZone)
            {
                transform.position = new Vector3(transform.position.x, Mathf.Abs(transform.position.y), transform.position.z);
                forceToApply = new Vector3(transform.position.x, transform.position.y * 50, transform.position.z);
                colorDebugForce += Color.cyan;

                sumForces += forceToApply;
                nbForcesApplied++;
            }

            if(transform.position.y < 1.5 && !_goToLandingZone)
            {
                transform.rotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                forceToApply = new Vector3(transform.position.x * 15, transform.position.y * 15, transform.position.z * 15);

                sumForces += forceToApply;
                nbForcesApplied++;
            }

            if (vecToOtherBoid.sqrMagnitude < _zoneAttraction * _zoneAttraction)
            {
                if(vecToOtherBoid.sqrMagnitude > _zoneAlignement * _zoneAlignement)
                {
                    forceToApply = vecToOtherBoid.normalized * _forceAttraction;
                    float distToOtherBoid = vecToOtherBoid.magnitude;
                    float normalizedDistanceToNextZone = ((distToOtherBoid - _zoneAlignement) /
                        (_zoneAttraction - _zoneAlignement));
                    float boostForce = 4 * normalizedDistanceToNextZone;


                    if(!_goToTarget)
                    {
                        boostForce *= boostForce;
                    }

                    forceToApply = vecToOtherBoid.normalized * _forceAttraction * boostForce;
                    colorDebugForce += Color.green;
                }
                else
                {
                    if(vecToOtherBoid.sqrMagnitude > _zoneRepulsion * _zoneRepulsion)
                    {
                        forceToApply = otherBoid._velocity.normalized * _forceAlignement;
                        colorDebugForce += Color.blue;
                    }
                    else
                    {
                        float distToOtherBoid = vecToOtherBoid.magnitude;
                        float normalizedDistanceToPreviousZone = 1 - (distToOtherBoid / _zoneRepulsion);
                        float boostForce = 4 * normalizedDistanceToPreviousZone;
                        forceToApply = vecToOtherBoid.normalized * -1 * (_forceRepulsion * boostForce);
                        colorDebugForce += Color.red;
                    }
                }

                sumForces += forceToApply;
                nbForcesApplied++;
            }
         }

        sumForces /= nbForcesApplied;

        if(_goToTarget)
        {
            Vector3 vecToTarget = _target - transform.position;

            if (vecToTarget.sqrMagnitude < 1)
            {
                _goToTarget = false;
            }
            else
            {
                Vector3 forceToTarget = vecToTarget.normalized * _forceTarget;
                sumForces += forceToTarget;
                colorDebugForce += Color.magenta;
                nbForcesApplied++;
            }

            if (_drawLines)
            {
                Debug.DrawLine(transform.position, _target, Color.magenta);
            }
        }

        if(_goToLandingZone)
        {
            Vector3 vecToLanding = _landingZone - transform.position;

            if(transform.position.y < 0.5)
            {
                _isLanded = true;
            }
            else
            {
                Vector3 forceToLandingZone = vecToLanding.normalized * _forceTarget;
                sumForces += forceToLandingZone;
                colorDebugForce += Color.red;
                nbForcesApplied++;
            }

            if (_drawLines)
            {
                Debug.DrawLine(transform.position, _landingZone, Color.red);
            }
        }
        else
        {
            _isLanded = false;
        }

        if (_drawLines)
        {
            Debug.DrawLine(transform.position, transform.position + sumForces, colorDebugForce/nbForcesApplied);
        }

        _velocity += -_velocity * 10 * Vector3.Angle(sumForces, _velocity) / 180.0f * Time.deltaTime;

        if(_isLanded)
        {
            _velocity = new Vector3(0, 0, 0);
            transform.position = new Vector3(transform.position.x, Mathf.Abs(transform.position.y), transform.position.z);
            transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
        }
        else
        {
            _velocity += sumForces * Time.deltaTime;
        }

        if(_velocity.sqrMagnitude > _maxSpeed * _maxSpeed)
        {
            _velocity = _velocity.normalized * _maxSpeed;
        }

        if(_velocity.sqrMagnitude < _minSpeed * _minSpeed)
        {
            _velocity = _velocity.normalized * _minSpeed;
        }

        if(_velocity.sqrMagnitude > 0)
        {
            transform.LookAt(transform.position + _velocity);
        }

        if (_drawLines)
        {
            Debug.DrawLine(transform.position, transform.position + _velocity, Color.blue);
        }

        transform.position += _velocity * Time.deltaTime;
    }

    private void OnDrawGizmosSelected()
    {
        if(_drawGizmos)
        {
            Gizmos.color = new Color(1, 0, 0, 1.0f);
            Gizmos.DrawWireSphere(transform.position, _zoneRepulsion);

            Gizmos.color = new Color(0, 1, 0, 1.0f);
            Gizmos.DrawWireSphere(transform.position, _zoneAlignement);

            Gizmos.color = new Color(0, 0, 1, 1.0f);
            Gizmos.DrawWireSphere(transform.position, _zoneAttraction);
        }
    }
}
