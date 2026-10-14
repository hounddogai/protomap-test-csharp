namespace Acme.Workforce.Compensation.PayBand.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal pay band audit.</summary>
public sealed record SeasonalPayBandAuditView(string CodeText, string Title, string ThresholdText, string Reviewer, string Revision, string StatusLabel)
{
    public static SeasonalPayBandAuditView ToSeasonalPayBandAuditView(SeasonalPayBandAudit item)
    {
        return new SeasonalPayBandAuditView(
            item.Code,
            item.Title,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 64),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalPayBandAuditView> ToSeasonalPayBandAuditViews(List<SeasonalPayBandAudit> items)
    {
        var views = new List<SeasonalPayBandAuditView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalPayBandAuditView(item));
        }
        return views;
    }

    public static double SumSeasonalPayBandAuditView(SeasonalPayBandAuditView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedRevision;
    }
}
