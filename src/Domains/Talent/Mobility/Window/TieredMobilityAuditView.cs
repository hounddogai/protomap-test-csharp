namespace Acme.Workforce.Talent.Mobility.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered mobility audit.</summary>
public sealed record TieredMobilityAuditView(string CodeText, string Name, string Active, string Sequence, string UpdatedAt, string EmployeeId, string Currency, string ThresholdText, string StatusLabel)
{
    public static TieredMobilityAuditView ToTieredMobilityAuditView(TieredMobilityAudit item)
    {
        return new TieredMobilityAuditView(
            item.Code,
            item.Name,
            $"{item.Active}",
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            Text.Truncate(item.Currency, 64),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredMobilityAuditView> ToTieredMobilityAuditViews(List<TieredMobilityAudit> items)
    {
        var views = new List<TieredMobilityAuditView>();
        foreach (var item in items)
        {
            views.Add(ToTieredMobilityAuditView(item));
        }
        return views;
    }

    public static double SumTieredMobilityAuditView(TieredMobilityAuditView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedUpdatedAt + parsedThreshold;
    }
}
