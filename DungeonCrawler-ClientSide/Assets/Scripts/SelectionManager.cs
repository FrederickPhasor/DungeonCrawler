using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private string selectableTag = "Selectable";

    private Transform _selection;
    [SerializeField] Camera m_MainCamera;

    void Update()
    {
        
        //var ray = m_MainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(m_MainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider != null)
        {
            Debug.Log("Target Position: " + hit.collider.gameObject.transform.position);
        }
        /*
        if (_selection != null)
        {
            var selectionImage = _selection.GetComponent<Image>();
            selectionImage.color = Color.yellow;
            _selection = null;
        }
        if (hit)
        {
            var selection = hit.transform;
            if (selection.CompareTag(selectableTag))
            {
                var selectionImage = selection.GetComponent<Image>();
                if (selectionImage != null)
                {
                    selectionImage.color = Color.yellow;
                    _selection = selection;
                }
            }
        }*/
    }
}
