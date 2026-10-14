namespace Acme.Workforce.Absence.Accrual.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual tier.</summary>
public sealed record AccrualTierView(string Code, string Label, string Description, string RetriesText, string LocationText, string Threshold, string Attempts, string Margin, string Unit, string StatusLabel)
{
    public static AccrualTierView ToAccrualTierView(AccrualTier item)
    {
        return new AccrualTierView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Description,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 40),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualTierView> ToAccrualTierViews(List<AccrualTier> items)
    {
        var views = new List<AccrualTierView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualTierView(item));
        }
        return views;
    }

    public static double SumAccrualTierView(AccrualTierView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedThreshold + (double)parsedAttempts;
    }
}
