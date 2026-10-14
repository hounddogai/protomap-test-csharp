namespace Acme.Workforce.Talent.Pool.PoolWindow;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending pool allocation.</summary>
public sealed record PendingPoolAllocationView(string Code, string Title, string ThresholdText, string ExternalRef, string Unit, string ManagerId, string RevisionText, string Attempts, string CostCenterText, string Currency, string Pinned, string StatusLabel)
{
    public static PendingPoolAllocationView ToPendingPoolAllocationView(PendingPoolAllocation item)
    {
        return new PendingPoolAllocationView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 80),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            Text.Truncate(item.Unit, 40),
            item.ManagerId,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            Text.Truncate(item.Currency, 80),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingPoolAllocationView> ToPendingPoolAllocationViews(List<PendingPoolAllocation> items)
    {
        var views = new List<PendingPoolAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToPendingPoolAllocationView(item));
        }
        return views;
    }

    public static double SumPendingPoolAllocationView(PendingPoolAllocationView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedRevision + (double)parsedAttempts;
    }
}
