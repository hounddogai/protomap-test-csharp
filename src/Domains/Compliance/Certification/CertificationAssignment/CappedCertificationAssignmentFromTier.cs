namespace Acme.Workforce.Compliance.Certification.CertificationAssignment;

using Acme.Workforce.Compliance.TrainingDue.Tier;

/// <summary>Functions of capped certification assignment from tier.</summary>
public static class CappedCertificationAssignmentFromTier
{
    public static Dictionary<string, double> SummarizeLenientTrainingDueTiersForAssignment(List<LenientTrainingDueTier> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<CappedCertificationAssignment> DeriveCappedCertificationAssignmentsFromTier(List<LenientTrainingDueTier> items)
    {
        var outValue = new List<CappedCertificationAssignment>();
        foreach (var item in items)
        {
            var derived = CappedCertificationAssignment.NewCappedCertificationAssignment(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyCappedCertificationAssignment(CappedCertificationAssignment item)
    {
        return WeightedTrainingDueCyclePolicy.NormalizeWeightedTrainingDueCycleCode(item.Code);
    }
}
