namespace Acme.Workforce.Compensation.EquityGrant.Schedule;

using Acme.Workforce.Compensation.MeritCycle.Draft;

/// <summary>Functions of equity grant segment from agreement.</summary>
public static class EquityGrantSegmentFromAgreement
{
    public static Dictionary<string, double> SummarizeMeritCycleAgreementsForSegment(List<MeritCycleAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<EquityGrantSegment> DeriveEquityGrantSegmentsFromAgreement(List<MeritCycleAgreement> items)
    {
        var outValue = new List<EquityGrantSegment>();
        foreach (var item in items)
        {
            var derived = EquityGrantSegment.NewEquityGrantSegment(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveEquityGrantSegment(EquityGrantSegment item)
    {
        return MeritCycleAgreementPolicy.NormalizeMeritCycleAgreementCode(item.Code);
    }
}
