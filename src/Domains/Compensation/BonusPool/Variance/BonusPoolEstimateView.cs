namespace Acme.Workforce.Compensation.BonusPool.Variance;

using System.Globalization;

/// <summary>How screens and exports show a bonus pool estimate.</summary>
public sealed record BonusPoolEstimateView(string Code, string Name, string EndsAtText, string Timezone, string Maximum, string StatusLabel)
{
    public static BonusPoolEstimateView ToBonusPoolEstimateView(BonusPoolEstimate item)
    {
        return new BonusPoolEstimateView(
            item.Code,
            item.Name,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolEstimateView> ToBonusPoolEstimateViews(List<BonusPoolEstimate> items)
    {
        var views = new List<BonusPoolEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolEstimateView(item));
        }
        return views;
    }

    public static double SumBonusPoolEstimateView(BonusPoolEstimateView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedMaximum;
    }
}
