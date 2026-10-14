namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped skill profile adjustment.</summary>
public sealed record CappedSkillProfileAdjustmentView(string CodeText, string NameText, string SalaryBandText, string SummaryText, string VersionText, string ManagerIdText, string Quantity, string WindowDays, string Active, string FloorValueText, string Segment, string StatusLabel)
{
    public static CappedSkillProfileAdjustmentView ToCappedSkillProfileAdjustmentView(CappedSkillProfileAdjustment item)
    {
        return new CappedSkillProfileAdjustmentView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 80),
            item.SalaryBand,
            item.Summary,
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 40),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedSkillProfileAdjustmentView> ToCappedSkillProfileAdjustmentViews(List<CappedSkillProfileAdjustment> items)
    {
        var views = new List<CappedSkillProfileAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToCappedSkillProfileAdjustmentView(item));
        }
        return views;
    }

    public static double SumCappedSkillProfileAdjustmentView(CappedSkillProfileAdjustmentView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedQuantity + (double)parsedWindowDays;
    }
}
