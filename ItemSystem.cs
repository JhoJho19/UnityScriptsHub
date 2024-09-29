using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Manages item data.
/// </summary>
public static class DataItem 
{
    public static Dictionary<string, Item> items = new Dictionary<string, Item>();

    private static string _filePath = Path.Combine(Application.persistentDataPath, "DataItem.json");

    public static Item GetItem(string key)
    {
        if (items.TryGetValue(key,out Item item))
        {
            return item;
        }
        return null;
    }

    public static void AddItem(Item item)
    {
        if (!items.ContainsKey(item.itemName))
        {
            items.Add(item.itemName, item);
        }
    }

    public static void LoadData()
    {
        if(File.Exists(_filePath))
        {
            string json = File.ReadAllText(_filePath);
            SerializableDictionary<string,Item> serializableDictionary = JsonUtility.FromJson<SerializableDictionary<string,Item>>(json);
            items = serializableDictionary.ToDictionary();
        }
        else
        {
            SaveData();
        }
    }

    public static void SaveData()
    {
        SerializableDictionary<string,Item> serializableDictionary = new SerializableDictionary<string, Item>(items);
        string json = JsonUtility.ToJson(serializableDictionary,true);
        File.WriteAllText(_filePath, json);
    }
}


/// <summary>
/// Manages the state of an item
/// </summary>
public class ItemController : MonoBehaviour
{
    public Item item;

    private void Awake()
    {
        Item currentItem = DataItem.GetItem(item.itemName);
        if (currentItem != null)
        {
            item = currentItem;
            if (item.isPickedUp)
            {
                Destroy(gameObject);
            }
        }
    }
}

/// <summary>
/// Responsible for collecting items. Must be on the player.
/// </summary>
public class ItemCollector : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Item"))
        {
            var itemController = collision.gameObject.GetComponent<ItemController>();
            if (itemController != null)
            {
                itemController.item.isPickedUp = true;
                DataItem.AddItem(itemController.item);
                Destroy(collision.gameObject);
            }
        }
    }
}

/// <summary>
/// SO for items. itemNames must be uniq.
/// </summary>
[CreateAssetMenu(menuName = "Custom/Item", fileName = "New Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite icon;
    [HideInInspector] public bool isPickedUp;
}
