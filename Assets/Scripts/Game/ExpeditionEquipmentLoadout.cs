public sealed class ExpeditionEquipmentLoadout
{
    public string MainArtifact;
    public string ProtectiveRelic;
    public string PillCauldron;
    public string TalismanCase;
    public string TalismanName;
    public string MedicineName;
    public int MainArtifactLevel;
    public int ProtectiveRelicLevel;
    public int PillCauldronLevel;
    public int TalismanCaseLevel;
    public int HealthBonus;
    public int AttackBonus;
    public int StressResistBonus;
    public int StartingTorchBonus;
    public int StartingSupplyBonus;
    public int TalismanCharges;
    public int MedicineCharges;
    public int MedicinePowerBonus;
    public int TalismanPowerBonus;

    public string ToSummary()
    {
        return "主法器：" + MainArtifact + "  +" + MainArtifactLevel + "\n" +
               "护身法器：" + ProtectiveRelic + "  +" + ProtectiveRelicLevel + "\n" +
               "丹炉：" + PillCauldron + "  +" + PillCauldronLevel + " / 丹囊：" + MedicineName + "\n" +
               "符匣：" + TalismanCase + "  +" + TalismanCaseLevel + " / 符箓：" + TalismanName;
    }
}
