using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class BoidManager : MonoBehaviour
{
    private static BoidManager _instance = null;
    public static BoidManager _sharedInstance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<BoidManager>();
            }

            return _instance;
        }
    }

    public Boid _prefabBoid;
    public float _nbBoids = 100;
    public float _startSpeed = 1;
    public float _startSpread = 10;

    public float _maxDistanceBoids = 30;

    public float _periodRetargetBoids = 6;
    public float _periodNoTargetBoids = 3;
    public float _timerRetargetBoids = 0;
    private bool _setTargetBoids = true;

    
    public float _timerCanLandingBoids = 0;
    public float _periodReLandZone = 10;
    public float _periodNoLandZone = 15;
    private bool _setLandingZone = true;

    public Camera _mainCam;
    public Transform _mainCamBaseTransform;
    public bool _isCameraFollow = false;
    private Boid _selectedBoid;
    public Toggle _camToggle;

    [Range(0, 10)]
    public float _offSetY;
    [Range(0,10)]
    public float _offSetZ;

    public Vector3 _ground;
    public Vector3 _skyPos;
    

    private List<Boid> _boids = new List<Boid>();

    public ReadOnlyCollection<Boid> roBoids
    {
        get
        {
            return new ReadOnlyCollection<Boid>(_boids);
        }
    }

    private void Start()
    {
        for(int i = 0; i< _nbBoids; i++)
        {
            Boid b = GameObject.Instantiate<Boid>(_prefabBoid);
            Vector3 positionBoid = Random.insideUnitSphere * _startSpread;
            positionBoid.y = Mathf.Abs(positionBoid.y);
            b.transform.position = positionBoid;
            b._velocity = (positionBoid - transform.position).normalized * _startSpeed;
            b.transform.parent = this.transform;
            b._maxSpeed *= Random.Range(0.95f, 1.05f);
            _boids.Add(b);
        }

        _selectedBoid = _boids[Random.Range(0, _boids.Count - 1)];
    }

    private void Update()
    {

        _timerRetargetBoids -= Time.deltaTime;

        if (_timerRetargetBoids <= 0)
        {
            if (!_setTargetBoids)
            {
                _timerRetargetBoids = _periodNoTargetBoids;
            }
            else
            {
                _timerRetargetBoids = _periodRetargetBoids;
            }

            Vector3 target = Random.insideUnitSphere * _maxDistanceBoids;
            target.y = Mathf.Max(Mathf.Abs(target.y), 10);

            foreach (Boid b in _boids)
            {
                  b._goToTarget = false;

                  if (_setTargetBoids && Random.Range(0.0f, 1.0f) < 0.3f)
                  {
                      b._target = target;
                      b._goToTarget = true;
                  }
            }

            _setTargetBoids = !_setTargetBoids;
        }

        _timerCanLandingBoids -= Time.deltaTime;

        if (_timerCanLandingBoids <= 0)
        {
            if (!_setLandingZone)
            {
                _timerCanLandingBoids = _periodNoLandZone;
            }
            else
            {
                _timerCanLandingBoids = _periodReLandZone;
            }

            Vector3 landZone = Random.insideUnitSphere * _maxDistanceBoids;
            landZone.y = 0;

            foreach (Boid b in _boids)
            {
                b._goToLandingZone = false;

                if (_setLandingZone)
                {
                    b._landingZone = landZone;
                    b._goToLandingZone = true;
                }
            }

            _setLandingZone = !_setLandingZone;
        }


        if (Input.GetButtonDown("Jump"))
        {
            _isCameraFollow = true;
        }

        if (Input.GetButtonDown("Cancel"))
        {
            _isCameraFollow = false;

            _selectedBoid = _boids[Random.Range(0, _boids.Count - 1)];
        }

        if (_isCameraFollow)
        {
            _camToggle.isOn = true;
            _mainCam.transform.parent = _selectedBoid.transform;
            _mainCam.transform.localPosition = new Vector3(0, _offSetY, -_offSetZ);
            _mainCam.transform.localRotation = Quaternion.Euler(Vector3.forward);

            if(Input.GetKeyDown(KeyCode.S))
            {
                _offSetZ++;
            }

            if(Input.GetKeyDown(KeyCode.Z) && _offSetZ > 0)
            {
                _offSetZ--;
            }
        }

        if (!_isCameraFollow)
        {
            _camToggle.isOn = false;
            _offSetZ = 0;
            _offSetY = 0;
            _mainCam.transform.parent = null;
            _mainCam.transform.position = _mainCamBaseTransform.position;
            _mainCam.transform.rotation = _mainCamBaseTransform.rotation;
        }
    }

    public void IsCameraFollow()
    {
        _selectedBoid = _boids[Random.Range(0, _boids.Count - 1)];

        if (_camToggle.isOn)
        {
            _isCameraFollow = true;
        }
        else
        {
            _isCameraFollow = false;
        }
    }
}
