namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using Acme.Workforce.Compliance.TrainingDue.DueTier;

/// <summary>Functions of default training due window from tier.</summary>
public static class DefaultTrainingDueWindowFromTier
{
    public static Dictionary<string, double> SummarizeBatchedTrainingDueTiersForWindow(List<BatchedTrainingDueTier> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<DefaultTrainingDueWindow> DeriveDefaultTrainingDueWindowsFromTier(List<BatchedTrainingDueTier> items)
    {
        var outValue = new List<DefaultTrainingDueWindow>();
        foreach (var item in items)
        {
            var derived = DefaultTrainingDueWindow.NewDefaultTrainingDueWindow(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateDefaultTrainingDueWindow(DefaultTrainingDueWindow item)
    {
        return RegionalTrainingDueExceptionPolicy.ClassifyRegionalTrainingDueException(item.MeasuredValue());
    }
}
