namespace Acme.Workforce.Talent.SuccessionPlan.Target;

using Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

/// <summary>Functions of lenient succession plan target from reservation.</summary>
public static class LenientSuccessionPlanTargetFromReservation
{
    public static Dictionary<string, double> SummarizeCappedMobilityReservationsForTarget(List<CappedMobilityReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<LenientSuccessionPlanTarget> DeriveLenientSuccessionPlanTargetsFromReservation(List<CappedMobilityReservation> items)
    {
        var outValue = new List<LenientSuccessionPlanTarget>();
        foreach (var item in items)
        {
            var derived = LenientSuccessionPlanTarget.NewLenientSuccessionPlanTarget(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveLenientSuccessionPlanTarget(LenientSuccessionPlanTarget item)
    {
        return MobilityProposalPolicy.NormalizeMobilityProposalCode(item.Code);
    }
}
