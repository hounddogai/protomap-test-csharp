namespace Acme.Workforce.Compliance.TrainingDue.Window;

using Acme.Workforce.Compliance.Exception.Review;

/// <summary>Functions of training due window from checkpoint.</summary>
public static class TrainingDueWindowFromCheckpoint
{
    public static Dictionary<string, double> SummarizeSeasonalExceptionCheckpointsForWindow(List<SeasonalExceptionCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<TrainingDueWindow> DeriveTrainingDueWindowsFromCheckpoint(List<SeasonalExceptionCheckpoint> items)
    {
        var outValue = new List<TrainingDueWindow>();
        foreach (var item in items)
        {
            var derived = TrainingDueWindow.NewTrainingDueWindow(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreTrainingDueWindow(TrainingDueWindow item)
    {
        return SeasonalExceptionCheckpointPolicy.ClassifySeasonalExceptionCheckpoint(item.ProjectedLoad());
    }
}
