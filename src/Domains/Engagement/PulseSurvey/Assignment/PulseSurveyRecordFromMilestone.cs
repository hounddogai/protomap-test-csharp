namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

using Acme.Workforce.Engagement.Feedback.Checkpoint;

/// <summary>Functions of pulse survey record from milestone.</summary>
public static class PulseSurveyRecordFromMilestone
{
    public static Dictionary<string, double> SummarizeSeasonalFeedbackMilestonesForRecord(List<SeasonalFeedbackMilestone> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<PulseSurveyRecord> DerivePulseSurveyRecordsFromMilestone(List<SeasonalFeedbackMilestone> items)
    {
        var outValue = new List<PulseSurveyRecord>();
        foreach (var item in items)
        {
            var derived = PulseSurveyRecord.NewPulseSurveyRecord(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitPulseSurveyRecord(PulseSurveyRecord item)
    {
        return SeasonalFeedbackMilestonePolicy.NormalizeSeasonalFeedbackMilestoneCode(item.Code);
    }
}
