namespace Acme.Workforce.Talent.SuccessionPlan.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling succession plan ledger.</summary>
public sealed record RollingSuccessionPlanLedgerView(string Code, string Label, string ExternalRef, string JobCode, string ChannelText, string Capacity, string StatusLabel)
{
    public static RollingSuccessionPlanLedgerView ToRollingSuccessionPlanLedgerView(RollingSuccessionPlanLedger item)
    {
        return new RollingSuccessionPlanLedgerView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.ExternalRef,
            item.JobCode,
            item.Channel,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingSuccessionPlanLedgerView> ToRollingSuccessionPlanLedgerViews(List<RollingSuccessionPlanLedger> items)
    {
        var views = new List<RollingSuccessionPlanLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToRollingSuccessionPlanLedgerView(item));
        }
        return views;
    }

    public static double SumRollingSuccessionPlanLedgerView(RollingSuccessionPlanLedgerView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedCapacity;
    }
}
