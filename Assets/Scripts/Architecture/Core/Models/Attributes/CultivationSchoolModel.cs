using QFramework;

/// <summary>
/// M13 SchoolModel：流派选择与流派加成。
/// </summary>
public sealed class CultivationSchoolModel : AbstractModel
{
    public readonly BindableProperty<SchoolType> SelectedSchool = new BindableProperty<SchoolType>(SchoolType.None);
    public readonly BindableProperty<int> SchoolLevel = new BindableProperty<int>(0);

    /// <summary>
    /// 流派是否已选择
    /// </summary>
    public bool HasSelectedSchool => SelectedSchool.Value != SchoolType.None;

    protected override void OnInit()
    {
    }

    /// <summary>
    /// 选择流派
    /// </summary>
    public void SelectSchool(SchoolType schoolType)
    {
        if (SelectedSchool.Value == schoolType)
        {
            return;
        }

        SelectedSchool.Value = schoolType;
        SchoolLevel.Value = 1;
    }

    /// <summary>
    /// 提升流派等级
    /// </summary>
    public void LevelUpSchool()
    {
        if (!HasSelectedSchool)
        {
            return;
        }

        SchoolLevel.Value++;
    }

    /// <summary>
    /// 重置流派（新游戏时调用）
    /// </summary>
    public void Reset()
    {
        SelectedSchool.Value = SchoolType.None;
        SchoolLevel.Value = 0;
    }
}
