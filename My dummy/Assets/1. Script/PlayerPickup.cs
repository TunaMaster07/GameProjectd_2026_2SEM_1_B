using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerPickup : MonoBehaviour
{

    [SerializeField] private Animator animator;                     // 플레이어 애니메이션 재생 & 플레이어를 줍는동안 움직이지 못하게하는게 목적. 
    [SerializeField] private PlayerController playerController;     // - 만약 플레이어가 채집중에 움직이지 못하게 할거라면 필요.
    [SerializeField] private InteractionUI interactionUI;
    [SerializeField] private float detectionRadius = 2f;
    [SerializeField] private float pickupDuration = 1.2f;
    [SerializeField] private float pickupMoment = 0.55f;

    private PickupItem targetItem;


    private Door targetDoor;
    private NPC targetNPC;

    private bool isPickingUp;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isPickingUp) return;

        // Player 주변에서 가장 가까운 아이템을 찾는다
        targetItem = FindClosestItem();
        targetDoor = FindClosestDoor();
        targetNPC = FindClosestNPC();

        ShowGuide();

        Keyboard keyboard = Keyboard.current;

        if (targetItem == null && targetDoor == null && targetNPC == null) return;


        // 가까운 아이템이 있을 떄 E키로 줍는다

        if(keyboard.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    private PickupItem FindClosestItem()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, ~0, QueryTriggerInteraction.Collide);
        PickupItem closestItem = null;
        float closestDistance = Mathf.Infinity;

        foreach(Collider itemCollider in colliders)
        {
            PickupItem item = itemCollider.GetComponentInParent<PickupItem>();

            if (item == null) continue;
            float distance = Vector3.Distance(transform.position, item.transform.position);

            if(distance < closestDistance)
            {
                closestDistance = distance;
                closestItem = item;
            }
        }

        return closestItem;
    }

    private Door FindClosestDoor()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, ~0, QueryTriggerInteraction.Collide);
        Door closestDoor = null;
        float closestDistance = Mathf.Infinity;

        foreach(Collider targetCollider in colliders)
        {
            Door item = targetCollider.GetComponentInParent<Door>();

            if (item == null) continue;
            float distance = Vector3.Distance(transform.position, item.transform.position);

            if(distance < closestDistance)
            {
                closestDistance = distance;
                closestDoor = item;
            }
        }

        return closestDoor;
    }

    private NPC FindClosestNPC()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, ~0, QueryTriggerInteraction.Collide);
        NPC closestNPC = null;
        float closestDistance = Mathf.Infinity;

        foreach(Collider targetCollider in colliders)
        {
            NPC item = targetCollider.GetComponentInParent<NPC>();

            if (item == null) continue;
            float distance = Vector3.Distance(transform.position, item.transform.position);

            if(distance < closestDistance)
            {
                closestDistance = distance;
                closestNPC = item;
            }
        }

        return closestNPC;
    }

    private void ShowGuide()
    {
        if(targetItem != null)
        {
            interactionUI.ShowGuide("[E] " + targetItem.GetItemName() + "줍기");
        }
        else if(targetDoor!= null)
        {
            interactionUI.ShowGuide(targetDoor.GetGuideText());
        }
        else if(targetNPC!= null)
        {
            interactionUI.ShowGuide(targetNPC.GetGuideText());
        }
        else
        {
            interactionUI.HideGuide();
        }
    }

    private void Interact()
    {
        if (targetItem != null)
        {
            StartCoroutine(PickupAnimation());
        }
        else if (targetDoor != null)
        {
            targetDoor.Interact();
        }
        else if (targetNPC != null)
        {
            interactionUI.ShowDialogue(targetNPC.GetNpcName(), targetNPC.GetMessage());
        }
    }

    private IEnumerator PickupAnimation()
    {
        isPickingUp = true;

        interactionUI.HideGuide();

        // 현재 찾은 아이템을 저장한다

        PickupItem itemToCollect = targetItem;

        // 아이템 방향으로 캐릭터를 돌려준다
        Vector3 itemDirection = itemToCollect.transform.position - transform.position;
        itemDirection.y = 0f;

        if(itemDirection.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(itemDirection);
        }

        // 줍기 상태로 변경하여 이동을 막는다 
        playerController.ChangeState(PlayerState.Pickup);
        // 전환선 없ㅎ이 PickUp01 상태로 이동한다 \
        animator.CrossFade("PickUp01", 0.1f);

        // 손이 아이템에 닿는 시점까지 기다린다

        yield return new WaitForSeconds(pickupMoment);

        // 실제로 아이템을 획득한다 

        if(itemToCollect != null)
        {
            itemToCollect.Collect();
        }

        // 남은 애니메이션 시간을 기다린다 \
        yield return new WaitForSeconds(pickupDuration - pickupMoment);

        // 이동 BlendTree로 돌아간다 
        animator.CrossFade("Blend Tree", 0.1f);

        // 노말 상태로 돌아가 이동을 허용한다
        playerController.ChangeState(PlayerState.Normal);

        targetItem = null;
        targetDoor = null;
        targetNPC = null;
        isPickingUp = false;
    }


}
