namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Functions of training due claim from assignment.</summary>
public static class TrainingDueClaimFromAssignment
{
    public static Dictionary<string, double> SummarizeLenientCertificationAssignmentsForClaim(List<LenientCertificationAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<TrainingDueClaim> DeriveTrainingDueClaimsFromAssignment(List<LenientCertificationAssignment> items)
    {
        var outValue = new List<TrainingDueClaim>();
        foreach (var item in items)
        {
            var derived = TrainingDueClaim.NewTrainingDueClaim(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateTrainingDueClaim(TrainingDueClaim item)
    {
        return LenientCertificationAssignmentPolicy.ClassifyLenientCertificationAssignment(item.MeasuredValue());
    }
}
