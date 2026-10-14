namespace Acme.Workforce.Absence.Accrual.AccrualTier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual manifest.</summary>
public sealed record AccrualManifestView(string Code, string Label, string RetriesText, string Limit, string Timezone, string OwnerId, string StatusLabel)
{
    public static AccrualManifestView ToAccrualManifestView(AccrualManifest item)
    {
        return new AccrualManifestView(
            item.Code,
            item.Label,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            Text.Truncate(item.OwnerId, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualManifestView> ToAccrualManifestViews(List<AccrualManifest> items)
    {
        var views = new List<AccrualManifestView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualManifestView(item));
        }
        return views;
    }

    public static double SumAccrualManifestView(AccrualManifestView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedLimit;
    }
}
