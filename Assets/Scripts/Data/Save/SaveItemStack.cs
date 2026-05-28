using System;

[Serializable]
public sealed class SaveItemStack
{
    public string itemId;
    public int quantity;

    public SaveItemStack()
    {
    }

    public SaveItemStack(string newItemId, int newQuantity)
    {
        itemId = newItemId;
        quantity = newQuantity;
    }
}
