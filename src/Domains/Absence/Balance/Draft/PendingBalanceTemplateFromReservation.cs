namespace Acme.Workforce.Absence.Balance.Draft;

using Acme.Workforce.Absence.LeaveRequest.Window;

/// <summary>Functions of pending balance template from reservation.</summary>
public static class PendingBalanceTemplateFromReservation
{
    public static Dictionary<string, double> SummarizeLenientLeaveRequestReservationsForTemplate(List<LenientLeaveRequestReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<PendingBalanceTemplate> DerivePendingBalanceTemplatesFromReservation(List<LenientLeaveRequestReservation> items)
    {
        var outValue = new List<PendingBalanceTemplate>();
        foreach (var item in items)
        {
            var derived = PendingBalanceTemplate.NewPendingBalanceTemplate(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizePendingBalanceTemplate(PendingBalanceTemplate item)
    {
        return FallbackLeaveRequestAdjustmentPolicy.NormalizeFallbackLeaveRequestAdjustmentCode(item.Code);
    }
}
