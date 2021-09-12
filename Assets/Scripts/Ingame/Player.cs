using UnityEngine;
using Zenject;

namespace MiniPlanetDefense
{
    /// <summary>
    /// The player. Can move on planets, jump, collect pickups and optionally also move while in the air.
    /// Dies when touching an enemy.
    /// </summary>
    public class Player : MonoBehaviour
    {
        [SerializeField] float moveSpeedOnPlanet;
        [SerializeField] float freeMovementSpeed = 10;
        [SerializeField] float jumpImpulse = 5;
        [SerializeField] float maxSpeed = 10f;
        [SerializeField] float onPlanetRadius = 0.1f;
        [SerializeField] Color colorOnPlanet = Color.yellow;
        [SerializeField] Color colorOffPlanet = Color.white;
        [SerializeField] Renderer mainRenderer;
        [SerializeField] TrailRenderer trailRenderer;
        [SerializeField] ParticleSystem deathParticleSystem;
        
        [Inject] PhysicsHelper physicsHelper;
        [Inject] Constants constants;
        [Inject] IngameUI ingameUI;
        [Inject] SoundManager soundManager;

        new Rigidbody2D rigidbody;

        float radius;


        Vector2 freeMoveDirection;

        bool isColoredOnPlanet;

        int score;

        bool destroyed;

        Planet previousPlanet;
        Planet currentPlanet;

        private bool isRotatingClockwise = false;

        public float rotatingSpeed = 5f;
        public float currentAngle;
        public float CurrentAngle => currentAngle;
        
        void Awake()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            physicsHelper = FindObjectOfType<PhysicsHelper>();
            soundManager = FindObjectOfType<SoundManager>();
            radius = transform.localScale.x / 2f;

            isColoredOnPlanet = false;
            RefreshColor();
        }

        void FixedUpdate()
        {
            
            // Gravitational attraction 
            if (currentPlanet == null)
            {
                rigidbody.AddForce(physicsHelper.GetGravityAtPosition(transform.position, radius));
                rigidbody.AddForce(freeMoveDirection * freeMovementSpeed);
            }
            
            // Cap max speed
            if (maxSpeed > 0)
            {
                var speedSqr = rigidbody.velocity.sqrMagnitude;
                if (speedSqr > maxSpeed * maxSpeed)
                {
                    rigidbody.velocity *= maxSpeed / Mathf.Sqrt(speedSqr);
                }
            }
        }

        void Update()
        { 
            currentPlanet = physicsHelper.GetCurrentPlanet(rigidbody.position, radius + onPlanetRadius);
            if ((currentPlanet != null) && (currentPlanet != previousPlanet))
            {
                soundManager.PlaySound(Sound.TouchPlanet);
            }

            previousPlanet = currentPlanet;
            
            if (currentPlanet == null)
            {
                FreelyMoveInDirections();
                RestrictPlayerPosition();
            }
            else
            {
                freeMoveDirection = Vector2.zero;
                OrbitPlanet();
                
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    var jumpForceDirection = -CalculateDeltaToPlanetCenter(currentPlanet).normalized;
                    rigidbody.velocity = jumpForceDirection * jumpImpulse;
                    currentPlanet = null;
                    soundManager.PlaySound(Sound.Jump);
                }
            }

            SetColoredOnPlanet(currentPlanet != null);
        }

        void FreelyMoveInDirections()
        {
            freeMoveDirection.x = Input.GetAxis("Horizontal");
            freeMoveDirection.y = Input.GetAxis("Vertical");
        }
        
        void RestrictPlayerPosition()
        {
            var distanceFromCenterSqr = rigidbody.position.sqrMagnitude;
            var maxDistanceFromCenter = constants.playfieldRadius - radius;
            if (distanceFromCenterSqr > maxDistanceFromCenter * maxDistanceFromCenter)
            {
                rigidbody.position *= maxDistanceFromCenter / Mathf.Sqrt(distanceFromCenterSqr);
            }
        }
        

        Vector3 CalculateDeltaToPlanetCenter(Planet planet)
        {
            return planet.transform.position - transform.position;
        }
        
        void SetColoredOnPlanet(bool value)
        {
            if (isColoredOnPlanet == value)
                return;

            isColoredOnPlanet = value;
            RefreshColor();
        }
        
        void RefreshColor()
        {
            var color = isColoredOnPlanet ? colorOnPlanet : colorOffPlanet;
            mainRenderer.material.color = color;
            trailRenderer.startColor = color;
            trailRenderer.endColor = color;
        }

        
        
        void OnCollisionEnter2D(Collision2D other)
        {
            var otherGameObject = other.gameObject;
            if (otherGameObject.CompareTag(Tag.Pickup))
            {
                var pickup = other.gameObject.GetComponent<Pickup>();
                pickup.Collect();

                soundManager.PlaySound(Sound.Pickup);
                
                score++;
                ingameUI.SetScore(score);
            }
            else if (otherGameObject.CompareTag(Tag.Enemy))
            {
                HitEnemy();
            }
        }

        void HitEnemy()
        {
            if (destroyed)
                return;

            deathParticleSystem.transform.parent = null;
            deathParticleSystem.Play();
            
            gameObject.SetActive(false);
            destroyed = true;

            ingameUI.ShowRestartScreen();
            
            soundManager.PlaySound(Sound.Death);
        }
        
        
        
        void OrbitPlanet()
        {
            float clockwiseMultiplier = (isRotatingClockwise) ? 1f : -1f;
        
            //Move object as orbit
            currentAngle += rotatingSpeed * Time.deltaTime * clockwiseMultiplier;
        
            if (CurrentAngle >= 360f)
            {
                currentAngle = CurrentAngle - 360f * clockwiseMultiplier;
            }
            transform.position = GetPositionOnCircle(currentAngle);
        
        }

        protected Vector2 GetPositionOnCircle(float angle)
        {
            Vector2 centerPos = currentPlanet.transform.position;
            float orbitRadius = currentPlanet.Radius + radius;
            float x = centerPos.x + Mathf.Cos(angle) * orbitRadius;
            float y = centerPos.y + Mathf.Sin(angle) * orbitRadius;
        
            return new Vector2(x, y);
        }
    }
}