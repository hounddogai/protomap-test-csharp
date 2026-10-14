namespace Acme.Workforce.Workplace.AccessBadge.Request;

using Acme.Workforce.Workplace.Visitor.Policy;

/// <summary>Functions of weighted access badge reservation from agreement.</summary>
public static class WeightedAccessBadgeReservationFromAgreement
{
    public static Dictionary<string, double> SummarizeVisitorAgreementsForReservation(List<VisitorAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<WeightedAccessBadgeReservation> DeriveWeightedAccessBadgeReservationsFromAgreement(List<VisitorAgreement> items)
    {
        var outValue = new List<WeightedAccessBadgeReservation>();
        foreach (var item in items)
        {
            var derived = WeightedAccessBadgeReservation.NewWeightedAccessBadgeReservation(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreWeightedAccessBadgeReservation(WeightedAccessBadgeReservation item)
    {
        return VisitorAgreementPolicy.ClassifyVisitorAgreement(item.EffectiveVolume());
    }
}
