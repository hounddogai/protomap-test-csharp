namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a succession plan manifest.</summary>
public sealed record SuccessionPlanManifestView(string Code, string LabelText, string SegmentText, string Notes, string Name, string CreatedAtText, string Currency, string JobCode, string Score, string ArchivedText, string StatusLabel)
{
    public static SuccessionPlanManifestView ToSuccessionPlanManifestView(SuccessionPlanManifest item)
    {
        return new SuccessionPlanManifestView(
            item.Code,
            item.Label,
            item.Segment,
            item.Notes,
            Text.Truncate(item.Name, 64),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            item.JobCode,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanManifestView> ToSuccessionPlanManifestViews(List<SuccessionPlanManifest> items)
    {
        var views = new List<SuccessionPlanManifestView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanManifestView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanManifestView(SuccessionPlanManifestView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedScore;
    }
}
