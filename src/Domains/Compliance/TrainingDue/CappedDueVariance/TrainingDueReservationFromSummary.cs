namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

using Acme.Workforce.Compliance.Attestation.Quota;

/// <summary>Functions of training due reservation from summary.</summary>
public static class TrainingDueReservationFromSummary
{
    public static Dictionary<string, double> SummarizeAttestationSummariesForReservation(List<AttestationSummary> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<TrainingDueReservation> DeriveTrainingDueReservationsFromSummary(List<AttestationSummary> items)
    {
        var outValue = new List<TrainingDueReservation>();
        foreach (var item in items)
        {
            var derived = TrainingDueReservation.NewTrainingDueReservation(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileTrainingDueReservation(TrainingDueReservation item)
    {
        return AttestationSummaryPolicy.NormalizeAttestationSummaryCode(item.Code);
    }
}
