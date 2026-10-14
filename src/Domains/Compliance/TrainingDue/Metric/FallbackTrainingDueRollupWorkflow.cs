namespace Acme.Workforce.Compliance.TrainingDue.Metric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback training due rollups through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackTrainingDueRollupService(IFallbackTrainingDueRollupStore Store, FallbackTrainingDueRollupValidator Validator, IClock Clock, List<string> Journal, FallbackTrainingDueRollupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback training due rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackTrainingDueRollupStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback training due rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackTrainingDueRollupStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback training due rollup {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of fallback training due rollup workflow.</summary>
public static class FallbackTrainingDueRollupWorkflow
{
    public static FallbackTrainingDueRollupService NewFallbackTrainingDueRollupService(IFallbackTrainingDueRollupStore store, IClock clock)
    {
        return new FallbackTrainingDueRollupService(
            store,
            FallbackTrainingDueRollupRules.DefaultFallbackTrainingDueRollupValidator(),
            clock,
            new List<string>(),
            FallbackTrainingDueRollupEngine.DefaultFallbackTrainingDueRollupEngine());
    }
}
