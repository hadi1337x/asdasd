using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    public bool isClicked = false; // by default
    public Image activeSprite;
    public TMP_Text count;
    public TMP_Text itemName;

    public void isBTNClicked()
    {
        isClicked = !isClicked;

        activeSprite.gameObject.SetActive(isClicked);
    }
}
