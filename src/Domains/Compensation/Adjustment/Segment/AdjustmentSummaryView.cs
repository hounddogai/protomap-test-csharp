namespace Acme.Workforce.Compensation.Adjustment.Segment;

using System.Globalization;

/// <summary>How screens and exports show a adjustment summary.</summary>
public sealed record AdjustmentSummaryView(string Code, string Label, string RateText, string GradeText, string RevisionText, string ExpiresAt, string Visible, string StatusLabel)
{
    public static AdjustmentSummaryView ToAdjustmentSummaryView(AdjustmentSummary item)
    {
        return new AdjustmentSummaryView(
            item.Code,
            item.Label,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentSummaryView> ToAdjustmentSummaryViews(List<AdjustmentSummary> items)
    {
        var views = new List<AdjustmentSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentSummaryView(item));
        }
        return views;
    }

    public static double SumAdjustmentSummaryView(AdjustmentSummaryView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedRevision + (double)parsedExpiresAt;
    }
}
