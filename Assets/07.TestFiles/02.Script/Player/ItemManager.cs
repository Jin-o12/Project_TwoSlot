using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    [Header("UI �� ���� ����")]
    public GameObject slot1IconObj;
    public GameObject slot2IconObj;
    public bool isSlot1Full = false;
    public bool isSlot2Full = false;
    private string slot1ItemName = "";
    private string slot2ItemName = "";

    [Header("������ �̹��� ���� (�� �������� �̰��� ���)")]
    public Sprite noiseLureSprite; // ���� �̳� 
    public Sprite flareSprite;     // ȭ�� ����ź 
    // public Sprite healkitSprite;   // ����ŰƮ

    void Start()
    {
        slot1IconObj.SetActive(false);
        slot2IconObj.SetActive(false);
    }

    void Update()
    {
        // [�׽�Ʈ��] ��ȹ���� ���õ� �⺻ ������ ���� �׽�Ʈ 
        if (Input.GetKeyDown(KeyCode.Alpha1)) GetItem(noiseLureSprite, "NoiseLure");
        if (Input.GetKeyDown(KeyCode.Alpha2)) GetItem(flareSprite, "Flare");

        // [�׽�Ʈ��] 1�� ���� ���(Q), 2�� ���� ���(E)
        if (Input.GetKeyDown(KeyCode.Q)) UseItem(1);
        if (Input.GetKeyDown(KeyCode.E)) UseItem(2);
    }

    // ������ ���� ���� �Լ�
    public void GetItem(Sprite itemImage, string itemName)
    {
        if (isSlot1Full == false)
        {
            slot1IconObj.GetComponent<Image>().sprite = itemImage;
            slot1IconObj.SetActive(true);
            slot1ItemName = itemName;
            isSlot1Full = true;
        }
        else if (isSlot2Full == false)
        {
            slot2IconObj.GetComponent<Image>().sprite = itemImage;
            slot2IconObj.SetActive(true);
            slot2ItemName = itemName;
            isSlot2Full = true;
        }
        else { Debug.Log("�κ��丮�� ���� á���ϴ�! (�ִ� 2����)"); }

    }

    // ������ ��� ���� �Լ�
    public void UseItem(int slotNumber)
    {
        if (slotNumber == 1 && isSlot1Full)
        {
            ApplyEffect(slot1ItemName); // �����ۺ� ���� ȿ�� ����
            slot1IconObj.SetActive(false);
            isSlot1Full = false;
            slot1ItemName = "";
        }
        else if (slotNumber == 2 && isSlot2Full)
        {
            ApplyEffect(slot2ItemName);
            slot2IconObj.SetActive(false);
            isSlot2Full = false;
            slot2ItemName = "";
        }
        else { Debug.Log(slotNumber + "�� ������ ����ֽ��ϴ�."); }
    }


    // ������ ȿ�� �з�
    // ���ο� �������� �߰��ߴٸ�, �Ʒ��� else if ���� �߰��Ͽ� ����

    void ApplyEffect(string itemName)
    {
        if (itemName == "NoiseLure") { ThrowNoiseLure(); }
        else if (itemName == "Flare") { UseFlare(); }
        else if (itemName == "Healkit") { UseHealkit(); }

        // [����] ���ο� ������ ȿ�� ����
        // else if (itemName == "NewItemName") { UseNewItem(); }
    }


    // [Ȯ�� ���� 2] ���� ������ ���� ȿ�� ����
    // �� �������� ������ � ���� ���� �޼ҵ带 �ۼ��ϴ� �����Դϴ�.


  
    void ThrowNoiseLure() // ���� �̳� [cite: 51]
    {
        Debug.Log("���� �̳� ��ô! ���� �ν��� �Ѱ����� �����ϴ�.");
        // TODO: ���� �����ϴ� ���� �� ���� ���� [cite: 51, 76]
    }

    
    void UseFlare() // ȭ�� ����ź [cite: 52]
    {
        Debug.Log("����ź �۵�! �Ȱ��� �Ͻ������� �����ϰ� �þ߸� Ȯ���մϴ�.");
        // TODO: �Ȱ� ���� ���� �� �� ���� ����ũ ����
    }

    
    void UseHealkit() // ������ �帵ũ
    {
        Debug.Log("������ �帵ũ ����! �̵��ӵ��� �Ͻ������� ���� �մϴ�!");
        // ĳ������ �̵��ӵ��� �����ð� �����ϴ� ȿ�� ����
    }

 
    /*void UseNewItem()
    {
        Debug.Log("���ο� �������� ȿ���� �ߵ��Ǿ����ϴ�!");
    }*/
}