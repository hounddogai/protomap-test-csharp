namespace Acme.Workforce.Compliance.Certification.CertificationForecast;

using Acme.Workforce.Compliance.Certification.CertificationSummary;

/// <summary>Functions of capped certification template from plan.</summary>
public static class CappedCertificationTemplateFromPlan
{
    public static Dictionary<string, double> SummarizeLenientCertificationPlansForTemplate(List<LenientCertificationPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<CappedCertificationTemplate> DeriveCappedCertificationTemplatesFromPlan(List<LenientCertificationPlan> items)
    {
        var outValue = new List<CappedCertificationTemplate>();
        foreach (var item in items)
        {
            var derived = CappedCertificationTemplate.NewCappedCertificationTemplate(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanCappedCertificationTemplate(CappedCertificationTemplate item)
    {
        return RollingCertificationSummaryPolicy.RollingCertificationSummaryWeight(6, true);
    }
}
