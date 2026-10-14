namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using System.Globalization;

/// <summary>How screens and exports show a primary adjustment preference.</summary>
public sealed record PrimaryAdjustmentPreferenceView(string CodeText, string LabelText, string Skill, string Location, string BadgeText, string Amount, string StatusLabel)
{
    public static PrimaryAdjustmentPreferenceView ToPrimaryAdjustmentPreferenceView(PrimaryAdjustmentPreference item)
    {
        return new PrimaryAdjustmentPreferenceView(
            item.Code,
            item.Label,
            item.Skill,
            item.Location,
            item.Badge,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryAdjustmentPreferenceView> ToPrimaryAdjustmentPreferenceViews(List<PrimaryAdjustmentPreference> items)
    {
        var views = new List<PrimaryAdjustmentPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryAdjustmentPreferenceView(item));
        }
        return views;
    }

    public static double SumPrimaryAdjustmentPreferenceView(PrimaryAdjustmentPreferenceView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
