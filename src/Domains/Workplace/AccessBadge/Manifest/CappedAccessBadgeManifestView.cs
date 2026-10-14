namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped access badge manifest.</summary>
public sealed record CappedAccessBadgeManifestView(string Code, string TitleText, string RetriesText, string Active, string Phone, string CategoryText, string WindowDays, string SalaryBand, string Currency, string Visible, string StatusLabel)
{
    public static CappedAccessBadgeManifestView ToCappedAccessBadgeManifestView(CappedAccessBadgeManifest item)
    {
        return new CappedAccessBadgeManifestView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            Text.Truncate(item.Phone, 80),
            item.Category,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 80),
            item.Currency,
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedAccessBadgeManifestView> ToCappedAccessBadgeManifestViews(List<CappedAccessBadgeManifest> items)
    {
        var views = new List<CappedAccessBadgeManifestView>();
        foreach (var item in items)
        {
            views.Add(ToCappedAccessBadgeManifestView(item));
        }
        return views;
    }

    public static double SumCappedAccessBadgeManifestView(CappedAccessBadgeManifestView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedWindowDays;
    }
}
