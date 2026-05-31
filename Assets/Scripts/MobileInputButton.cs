using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileInputButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum TipeInputMobile { Maju, Mundur, Kiri, Kanan, Rem }
    
    public TipeInputMobile tipeInput;

    private Image targetImage;
    private Color warnaAsli;
    private Color warnaDitekan = new Color(0.6f, 0.6f, 0.6f, 0.8f);

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        if (targetImage != null)
        {
            warnaAsli = targetImage.color;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        KirimSinyalInput(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        KirimSinyalInput(false);
    }

    private void KirimSinyalInput(bool isPressed)
    {
        if (targetImage != null)
        {
            targetImage.color = isPressed ? warnaDitekan : warnaAsli;
        }

        if (CarControllerPro.ActiveInstance != null)
        {
            switch (tipeInput)
            {
                case TipeInputMobile.Maju:
                    CarControllerPro.ActiveInstance.SetInputMaju(isPressed);
                    break;
                case TipeInputMobile.Mundur:
                    CarControllerPro.ActiveInstance.SetInputMundur(isPressed);
                    break;
                case TipeInputMobile.Kiri:
                    CarControllerPro.ActiveInstance.SetInputKiri(isPressed);
                    break;
                case TipeInputMobile.Kanan:
                    CarControllerPro.ActiveInstance.SetInputKanan(isPressed);
                    break;
                case TipeInputMobile.Rem:
                    CarControllerPro.ActiveInstance.SetInputRem(isPressed);
                    break;
            }
        }
    }

    private void OnDisable()
    {
        KirimSinyalInput(false);
    }
}