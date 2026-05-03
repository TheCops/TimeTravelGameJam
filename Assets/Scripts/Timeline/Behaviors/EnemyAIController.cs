using UnityEngine;

namespace TimeTravelBanana.Timeline.Behaviors
{
    public class EnemyAIController : MonoBehaviour
    {
        public enum AIState { Patrol, Alert, Attack, Cooldown }

        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float alertRange = 5f;
        [SerializeField] private float alertDuration = 1.5f;
        [SerializeField] private float attackWindup = 0.8f;
        [SerializeField] private float cooldownDuration = 2f;
        [SerializeField] private Transform target;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private TimelineSpawner spawner;

        private AIState state = AIState.Patrol;
        private float stateTimer;
        private Vector2 patrolDirection = Vector2.right;
        private bool firedThisAttack;

        public AIState State { get => state; set => state = value; }
        public float StateTimer { get => stateTimer; set => stateTimer = value; }
        public Vector2 PatrolDirection { get => patrolDirection; set => patrolDirection = value; }
        public bool FiredThisAttack { get => firedThisAttack; set => firedThisAttack = value; }

        private void Update()
        {
            stateTimer += Time.deltaTime;
            switch (state)
            {
                case AIState.Patrol:
                    transform.Translate(patrolDirection * (patrolSpeed * Time.deltaTime));
                    if (target != null && Vector2.Distance(transform.position, target.position) < alertRange)
                        TransitionTo(AIState.Alert);
                    break;
                case AIState.Alert:
                    if (stateTimer >= alertDuration) TransitionTo(AIState.Attack);
                    break;
                case AIState.Attack:
                    if (!firedThisAttack && stateTimer >= attackWindup * 0.5f)
                    {
                        FireProjectile();
                        firedThisAttack = true;
                    }
                    if (stateTimer >= attackWindup) TransitionTo(AIState.Cooldown);
                    break;
                case AIState.Cooldown:
                    if (stateTimer >= cooldownDuration) TransitionTo(AIState.Patrol);
                    break;
            }
        }

        private void TransitionTo(AIState next)
        {
            state = next;
            stateTimer = 0f;
            if (next == AIState.Attack) firedThisAttack = false;
        }

        private void FireProjectile()
        {
            if (spawner == null || projectilePrefab == null || target == null) return;
            Vector2 dir = ((Vector2)(target.position - transform.position)).normalized;
            spawner.Spawn(projectilePrefab, transform.position, Quaternion.FromToRotation(Vector3.right, dir));
        }
    }
}
