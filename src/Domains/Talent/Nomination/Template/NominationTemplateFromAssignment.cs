namespace Acme.Workforce.Talent.Nomination.Template;

using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Functions of nomination template from assignment.</summary>
public static class NominationTemplateFromAssignment
{
    public static Dictionary<string, double> SummarizeSuccessionPlanAssignmentsForTemplate(List<SuccessionPlanAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<NominationTemplate> DeriveNominationTemplatesFromAssignment(List<SuccessionPlanAssignment> items)
    {
        var outValue = new List<NominationTemplate>();
        foreach (var item in items)
        {
            var derived = NominationTemplate.NewNominationTemplate(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyNominationTemplate(NominationTemplate item)
    {
        return RollingSuccessionPlanOverridePolicy.RollingSuccessionPlanOverrideWeight(9, true);
    }
}
