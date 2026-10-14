namespace Acme.Workforce.Compliance.TrainingDue.Quota;

using Acme.Workforce.Compliance.Certification.CertificationAssignment;

/// <summary>Functions of regional training due reservation from assignment.</summary>
public static class RegionalTrainingDueReservationFromAssignment
{
    public static Dictionary<string, double> SummarizeCappedCertificationAssignmentsForReservation(List<CappedCertificationAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<RegionalTrainingDueReservation> DeriveRegionalTrainingDueReservationsFromAssignment(List<CappedCertificationAssignment> items)
    {
        var outValue = new List<RegionalTrainingDueReservation>();
        foreach (var item in items)
        {
            var derived = RegionalTrainingDueReservation.NewRegionalTrainingDueReservation(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignRegionalTrainingDueReservation(RegionalTrainingDueReservation item)
    {
        return TieredCertificationSummaryPolicy.TieredCertificationSummaryWeight(8, true);
    }
}
