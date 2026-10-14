namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional bonus pool override.</summary>
public sealed record RegionalBonusPoolOverrideView(string Code, string LabelText, string Category, string Fte, string Revision, string Retries, string CurrencyText, string ExternalRef, string Segment, string Unit, string DurationMinutes, string StatusLabel)
{
    public static RegionalBonusPoolOverrideView ToRegionalBonusPoolOverrideView(RegionalBonusPoolOverride item)
    {
        return new RegionalBonusPoolOverrideView(
            item.Code,
            item.Label,
            Text.Truncate(item.Category, 120),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            item.ExternalRef,
            item.Segment,
            item.Unit,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalBonusPoolOverrideView> ToRegionalBonusPoolOverrideViews(List<RegionalBonusPoolOverride> items)
    {
        var views = new List<RegionalBonusPoolOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalBonusPoolOverrideView(item));
        }
        return views;
    }

    public static double SumRegionalBonusPoolOverrideView(RegionalBonusPoolOverrideView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedRevision + (double)parsedRetries;
    }
}
