using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class OrderMenu : MonoBehaviour
{
    [Header("Orders list")]
    [SerializeField] private List<Order> orders;

    [Header("Settings")]
    [SerializeField] private Color baseColor;
    [SerializeField] private Color chooseColor;

    [SerializeField][Range(1, 3)] private float radius = 3.0f;
    [SerializeField][Range(0, 1f)] private float selectedOuterRadius = 0.1f;

    [SerializeField][Range(0f, 30f)] private float angleBetween;

    [Header("Prefabs")]
    [SerializeField] private Transform canvas;
    [SerializeField] private Transform sectionPrefab;
    [SerializeField] private Image iconPrefab;

    private Vector3 centerPoint;
    private int currentSelectedSection;

    private Order currentOrder;
    public Order CurrentOrder => currentOrder;

    private Transform agent;
    // Start is called before the first frame update
    void Start()
    {
        SpawnMenu();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePos();
        HighlightSection();
    }

    private void SpawnMenu()
    {
        for (int i = 0; i < orders.Count; i++)
        {
            //add section
            float angle = -i * 360.0f / orders.Count - angleBetween / 2;
            Vector3 vecAngel = new Vector3(0f, 0f, angle);

            Transform section = Instantiate(sectionPrefab, canvas);
            agent = canvas.gameObject.GetComponentInParent<IAgent>().GetGameObject().transform;

            Image image = section.GetComponent<Image>();

            section.position = canvas.position;
            section.localEulerAngles = vecAngel;

            float fillAmount = 1f / orders.Count - angleBetween / 360f;
            image.fillAmount = fillAmount;

            section.gameObject.SetActive(true);

            Order nowOrder = orders[i];
            nowOrder.sectionObj = section.gameObject;
            orders[i] = nowOrder;

            //add icon
            Transform icon = Instantiate(iconPrefab, section).transform;
            Image iconImage = icon.GetComponent<Image>();
            iconImage.sprite = orders[i].image;

            float angleSection = 360.0f * fillAmount;

            Vector3 vecStart = new Vector3(Mathf.Cos(angleSection),
                                           Mathf.Sin(angleSection),
                                           0.0f);
            Vector3 vecEnd = new Vector3(0.0f, -1.0f, 0.0f);
            Vector3 dirVec = Vector3.Normalize(vecStart +  vecEnd);

            float dist = Vector3.Distance(icon.position, section.position);

            icon.localPosition = dirVec * dist; //goffy ahh numbers
            icon.localEulerAngles = -vecAngel;

            icon.gameObject.SetActive(true);
        }
    }

    private void UpdatePos()
    {
        centerPoint = Camera.main.WorldToScreenPoint(agent.position);
        foreach (Order order in orders)
        {
            order.sectionObj.transform.position = centerPoint;
        }
    }

    private void HighlightSection()
    {
        Vector3 centerToMouse = Input.mousePosition - centerPoint;
        Vector3 centerToMouseProj = Vector3.ProjectOnPlane(centerToMouse, canvas.forward);

        float angle = Vector3.SignedAngle(canvas.up, -centerToMouseProj, -canvas.forward);
        if (angle < 0)
            angle += 360;
        
        currentSelectedSection = (int)angle * orders.Count / 360;

        for(int i = 0; i < orders.Count; i++)
        {
            if(i == currentSelectedSection)
            {
                currentOrder = orders[i];

                orders[i].sectionObj.GetComponent<Image>().color = chooseColor;
                orders[i].sectionObj.transform.localScale = (radius + selectedOuterRadius) * Vector3.one;
            }
            else
            {
                orders[i].sectionObj.GetComponent<Image>().color = baseColor;
                orders[i].sectionObj.transform.localScale = radius * Vector3.one;
            }
        }
    }
}

[Serializable]
public struct Order
{
    public GameObject sectionObj;
    public Orders order;
    public Sprite image;
}