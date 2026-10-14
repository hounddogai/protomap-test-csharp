namespace Acme.Workforce.Absence.Accrual.AccrualManifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary accrual manifest.</summary>
public sealed record PrimaryAccrualManifestView(string Code, string Title, string Archived, string Currency, string ThresholdText, string StatusLabel)
{
    public static PrimaryAccrualManifestView ToPrimaryAccrualManifestView(PrimaryAccrualManifest item)
    {
        return new PrimaryAccrualManifestView(
            item.Code,
            Text.Truncate(item.Title, 64),
            $"{item.Archived}",
            Text.Truncate(item.Currency, 40),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryAccrualManifestView> ToPrimaryAccrualManifestViews(List<PrimaryAccrualManifest> items)
    {
        var views = new List<PrimaryAccrualManifestView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryAccrualManifestView(item));
        }
        return views;
    }

    public static double SumPrimaryAccrualManifestView(PrimaryAccrualManifestView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return parsedThreshold;
    }
}
