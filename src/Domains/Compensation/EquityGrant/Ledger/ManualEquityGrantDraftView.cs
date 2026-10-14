namespace Acme.Workforce.Compensation.EquityGrant.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual equity grant draft.</summary>
public sealed record ManualEquityGrantDraftView(string CodeText, string Title, string Version, string Budget, string ChecksumText, string Pinned, string RatingText, string StatusLabel)
{
    public static ManualEquityGrantDraftView ToManualEquityGrantDraftView(ManualEquityGrantDraft item)
    {
        return new ManualEquityGrantDraftView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            $"{item.Pinned}",
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualEquityGrantDraftView> ToManualEquityGrantDraftViews(List<ManualEquityGrantDraft> items)
    {
        var views = new List<ManualEquityGrantDraftView>();
        foreach (var item in items)
        {
            views.Add(ToManualEquityGrantDraftView(item));
        }
        return views;
    }

    public static double SumManualEquityGrantDraftView(ManualEquityGrantDraftView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedBudget + parsedRating;
    }
}
