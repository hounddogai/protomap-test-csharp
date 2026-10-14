namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adjustment manifest.</summary>
public sealed record AdjustmentManifestView(string CodeText, string Title, string ScoreText, string DurationMinutesText, string ContactEmailText, string StatusLabel)
{
    public static AdjustmentManifestView ToAdjustmentManifestView(AdjustmentManifest item)
    {
        return new AdjustmentManifestView(
            item.Code,
            item.Title,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentManifestView> ToAdjustmentManifestViews(List<AdjustmentManifest> items)
    {
        var views = new List<AdjustmentManifestView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentManifestView(item));
        }
        return views;
    }

    public static double SumAdjustmentManifestView(AdjustmentManifestView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedDurationMinutes;
    }
}
