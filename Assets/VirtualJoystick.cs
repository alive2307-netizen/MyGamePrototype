using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public RectTransform handle;
    private Vector2 inputVector = Vector2.zero; // Обнуляем при старте

    void Start()
    {
        // Убеждаемся, что рычажок стоит по центру
        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out localPoint);

        Vector2 radius = GetComponent<RectTransform>().sizeDelta / 2;
        inputVector = localPoint / radius;
        
        if (inputVector.magnitude > 1) inputVector = inputVector.normalized;

        handle.anchoredPosition = inputVector * radius;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero; 
    }

    public void OnPointerDown(PointerEventData eventData) { }

    public Vector2 GetInput()
    {
        // Возвращаем 0, если мышь не зажата над джойстиком
        return inputVector;
    }
}