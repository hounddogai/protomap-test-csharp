namespace Acme.Workforce.Absence.Balance.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a balance ledger.</summary>
public sealed record BalanceLedgerView(string Code, string LabelText, string DurationMinutes, string Segment, string PriorityText, string StatusLabel)
{
    public static BalanceLedgerView ToBalanceLedgerView(BalanceLedger item)
    {
        return new BalanceLedgerView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 40),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceLedgerView> ToBalanceLedgerViews(List<BalanceLedger> items)
    {
        var views = new List<BalanceLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceLedgerView(item));
        }
        return views;
    }

    public static double SumBalanceLedgerView(BalanceLedgerView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedPriority;
    }
}
