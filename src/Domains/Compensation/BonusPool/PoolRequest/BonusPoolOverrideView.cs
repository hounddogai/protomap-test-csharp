namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

using System.Globalization;

/// <summary>How screens and exports show a bonus pool override.</summary>
public sealed record BonusPoolOverrideView(string CodeText, string Label, string CategoryText, string UpdatedAt, string Description, string DurationMinutes, string Minimum, string RevisionText, string StatusLabel)
{
    public static BonusPoolOverrideView ToBonusPoolOverrideView(BonusPoolOverride item)
    {
        return new BonusPoolOverrideView(
            item.Code,
            item.Label,
            item.Category,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolOverrideView> ToBonusPoolOverrideViews(List<BonusPoolOverride> items)
    {
        var views = new List<BonusPoolOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolOverrideView(item));
        }
        return views;
    }

    public static double SumBonusPoolOverrideView(BonusPoolOverrideView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedDurationMinutes + parsedMinimum;
    }
}
