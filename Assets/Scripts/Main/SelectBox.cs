using UnityEngine;
using UnityEngine.EventSystems;

public class SelectBox : Component, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        GameLogicSupervisor.Instance.JengaCtrl.NextDestinationReceiver(transform.position, transform.rotation);
        gameObject.SetActive(false);
    }
}
