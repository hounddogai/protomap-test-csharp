namespace Acme.Workforce.Talent.Nomination.Template;

using Acme.Workforce.Talent.Nomination.Adjustment;

/// <summary>Functions of nomination template from agreement.</summary>
public static class NominationTemplateFromAgreement
{
    public static Dictionary<string, double> SummarizeNominationAgreementsForTemplate(List<NominationAgreement> items)
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

    public static List<NominationTemplate> DeriveNominationTemplatesFromAgreement(List<NominationAgreement> items)
    {
        var outValue = new List<NominationTemplate>();
        foreach (var item in items)
        {
            var derived = NominationTemplate.NewNominationTemplate(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanNominationTemplate(NominationTemplate item)
    {
        return SeasonalNominationLedgerPolicy.SeasonalNominationLedgerBucket(item.WeightedLoad());
    }
}
