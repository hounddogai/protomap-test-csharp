namespace Acme.Workforce.Payrollops.PayRun.Schedule;

using Acme.Workforce.Payrollops.TaxForm.Assignment;

/// <summary>Functions of pay run reservation from assignment.</summary>
public static class PayRunReservationFromAssignment
{
    public static Dictionary<string, double> SummarizeTaxFormAssignmentsForReservation(List<TaxFormAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<PayRunReservation> DerivePayRunReservationsFromAssignment(List<TaxFormAssignment> items)
    {
        var outValue = new List<PayRunReservation>();
        foreach (var item in items)
        {
            var derived = PayRunReservation.NewPayRunReservation(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettlePayRunReservation(PayRunReservation item)
    {
        return TaxFormAssignmentPolicy.TaxFormAssignmentWeight(5, true);
    }
}
