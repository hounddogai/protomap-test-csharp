namespace Acme.Workforce.Compliance.TrainingDue.Summary;

using Acme.Workforce.Compliance.Exception.ExceptionAssignment;

/// <summary>Functions of training due milestone from assignment.</summary>
public static class TrainingDueMilestoneFromAssignment
{
    public static Dictionary<string, double> SummarizeScheduledExceptionAssignmentsForMilestone(List<ScheduledExceptionAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<TrainingDueMilestone> DeriveTrainingDueMilestonesFromAssignment(List<ScheduledExceptionAssignment> items)
    {
        var outValue = new List<TrainingDueMilestone>();
        foreach (var item in items)
        {
            var derived = TrainingDueMilestone.NewTrainingDueMilestone(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyTrainingDueMilestone(TrainingDueMilestone item)
    {
        return ExceptionVariancePolicy.NormalizeExceptionVarianceCode(item.Code);
    }
}
