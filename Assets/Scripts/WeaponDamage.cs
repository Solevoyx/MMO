using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class WeaponDamage : MonoBehaviourPun
{
    [System.Serializable]
    public class DamagePhase
    {
        public int damage;
        public float delay1;       // Задержка перед началом этой конкретной фазы урона
        public float delayAttack;  // Сколько времени эта фаза активна (окно нанесения урона)
    }

    [System.Serializable]
    public class AttackDamageData
    {
        public int id;
        public DamagePhase[] phases; // Новый вложенный массив для фаз атаки
    }

    [Header("Attacks")]
    public AttackDamageData[] attacks;

    private bool canDealDamage;
    private HashSet<PlayerHealth> hitTargets = new();

    private PlayerTeam myTeam;

    private Coroutine attackRoutine;
    private DamagePhase currentPhase; // Теперь отслеживаем текущую активную фазу

    private void Awake()
    {
        myTeam = GetComponentInParent<PlayerTeam>();

        var controller = GetComponentInParent<TopDownCharacterController>();
        if (controller != null)
        {
            controller.OnAttackEvent += OnAttackEvent;
        }
    }

    private void OnDestroy()
    {
        var controller = GetComponentInParent<TopDownCharacterController>();
        if (controller != null)
        {
            controller.OnAttackEvent -= OnAttackEvent;
        }
    }

    private void OnAttackEvent(int id)
    {
        for (int i = 0; i < attacks.Length; i++)
        {
            if (attacks[i].id == id)
            {
                StartAttack(attacks[i]);
                return;
            }
        }
    }

    private void StartAttack(AttackDamageData data)
    {
        // Если предыдущая атака (или серия ударов) еще идет, прерываем её
        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(AttackFlow(data));
    }

    private IEnumerator AttackFlow(AttackDamageData data)
    {
        // Проходим по очереди через каждую фазу урона, настроенную в инспекторе
        foreach (var phase in data.phases)
        {
            currentPhase = phase;
            canDealDamage = false;
            hitTargets.Clear(); // Сбрасываем список целей для новой фазы

            // Ждем задержку до включения триггера
            yield return new WaitForSeconds(phase.delay1);

            canDealDamage = true;
            hitTargets.Clear(); // Дополнительный сброс на случай, если кто-то зашел в триггер во время деления

            // Ждем пока фаза нанесения урона активна
            yield return new WaitForSeconds(phase.delayAttack);

            canDealDamage = false;
        }

        // Как только все фазы прошли, обнуляем ссылки
        attackRoutine = null;
        currentPhase = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        if (!canDealDamage) return;
        if (currentPhase == null) return; // Проверяем существование фазы, а не всей атаки

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null) return;

        // НЕ НАНОСИТЬ УРОН СЕБЕ
        if (health.transform.root == transform.root)
            return;

        PlayerTeam enemyTeam = other.GetComponentInParent<PlayerTeam>();

        string my = myTeam != null ? myTeam.CurrentTeam : "";
        string enemy = enemyTeam != null ? enemyTeam.CurrentTeam : "";

        // НЕ БИТЬ СОЮЗНИКОВ
        if (!string.IsNullOrEmpty(my) &&
            !string.IsNullOrEmpty(enemy) &&
            my == enemy)
        {
            return;
        }

        // НЕ БИТЬ ОДНУ ЦЕЛЬ НЕСКОЛЬКО РАЗ ЗА КОНКРЕТНУЮ ФАЗУ
        if (hitTargets.Contains(health))
            return;

        hitTargets.Add(health);

        // Наносим урон из текущей активной фазы
        health.photonView.RPC(
            nameof(PlayerHealth.TakeDamageRPC),
            health.photonView.Owner,
            currentPhase.damage
        );
    }
}