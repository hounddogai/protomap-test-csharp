namespace Acme.Workforce.Engagement.Recognition.Score;

using Acme.Workforce.Engagement.PulseSurvey.Audit;

/// <summary>Functions of recognition score from checkpoint.</summary>
public static class RecognitionScoreFromCheckpoint
{
    public static Dictionary<string, double> SummarizePulseSurveyCheckpointsForScore(List<PulseSurveyCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<RecognitionScore> DeriveRecognitionScoresFromCheckpoint(List<PulseSurveyCheckpoint> items)
    {
        var outValue = new List<RecognitionScore>();
        foreach (var item in items)
        {
            var derived = RecognitionScore.NewRecognitionScore(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanRecognitionScore(RecognitionScore item)
    {
        return FallbackPulseSurveySegmentPolicy.FallbackPulseSurveySegmentWeight(2, true);
    }
}
