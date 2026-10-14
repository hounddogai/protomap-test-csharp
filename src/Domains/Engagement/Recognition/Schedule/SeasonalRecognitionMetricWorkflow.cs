namespace Acme.Workforce.Engagement.Recognition.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal recognition metrics through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalRecognitionMetricManager(ISeasonalRecognitionMetricStore Store, SeasonalRecognitionMetricValidator Validator, IClock Clock, List<string> Journal, SeasonalRecognitionMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal recognition metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalRecognitionMetricStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal recognition metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalRecognitionMetricStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal recognition metric {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            var problems = Validator.Validate(item);
            if (problems.Count == 0)
            {
                continue;
            }
            Note($"{item.Code} needs attention: {string.Join(", ", problems)}");
            changed += 1;
        }
        return changed;
    }
}

/// <summary>Functions of seasonal recognition metric workflow.</summary>
public static class SeasonalRecognitionMetricWorkflow
{
    public static SeasonalRecognitionMetricManager NewSeasonalRecognitionMetricManager(ISeasonalRecognitionMetricStore store, IClock clock)
    {
        return new SeasonalRecognitionMetricManager(
            store,
            SeasonalRecognitionMetricRules.DefaultSeasonalRecognitionMetricValidator(),
            clock,
            new List<string>(),
            SeasonalRecognitionMetricEngine.DefaultSeasonalRecognitionMetricEngine());
    }
}
