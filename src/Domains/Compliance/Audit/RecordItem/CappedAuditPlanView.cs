namespace Acme.Workforce.Compliance.Audit.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped audit plan.</summary>
public sealed record CappedAuditPlanView(string Code, string Label, string Ratio, string Revision, string UnitText, string DurationMinutes, string ApprovedText, string Minimum, string DescriptionText, string StatusLabel)
{
    public static CappedAuditPlanView ToCappedAuditPlanView(CappedAuditPlan item)
    {
        return new CappedAuditPlanView(
            item.Code,
            item.Label,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedAuditPlanView> ToCappedAuditPlanViews(List<CappedAuditPlan> items)
    {
        var views = new List<CappedAuditPlanView>();
        foreach (var item in items)
        {
            views.Add(ToCappedAuditPlanView(item));
        }
        return views;
    }

    public static double SumCappedAuditPlanView(CappedAuditPlanView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedRevision + (double)parsedDurationMinutes;
    }
}
