namespace Acme.Workforce.Engagement.ActionPlan.Preference;

using Acme.Workforce.Engagement.PulseSurvey.Exception;

/// <summary>Functions of action plan assignment from exception.</summary>
public static class ActionPlanAssignmentFromException
{
    public static Dictionary<string, double> SummarizePulseSurveyExceptionsForAssignment(List<PulseSurveyException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<ActionPlanAssignment> DeriveActionPlanAssignmentsFromException(List<PulseSurveyException> items)
    {
        var outValue = new List<ActionPlanAssignment>();
        foreach (var item in items)
        {
            var derived = ActionPlanAssignment.NewActionPlanAssignment(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankActionPlanAssignment(ActionPlanAssignment item)
    {
        return PulseSurveyExceptionPolicy.PulseSurveyExceptionWeight(9, true);
    }
}
