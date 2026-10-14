namespace Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive reorg baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveReorgBaselineService(IAdaptiveReorgBaselineStore Store, AdaptiveReorgBaselineValidator Validator, IClock Clock, List<string> Journal, AdaptiveReorgBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive reorg baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveReorgBaselineStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive reorg baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveReorgBaselineStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive reorg baseline {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of adaptive reorg baseline workflow.</summary>
public static class AdaptiveReorgBaselineWorkflow
{
    public static AdaptiveReorgBaselineService NewAdaptiveReorgBaselineService(IAdaptiveReorgBaselineStore store, IClock clock)
    {
        return new AdaptiveReorgBaselineService(
            store,
            AdaptiveReorgBaselineRules.DefaultAdaptiveReorgBaselineValidator(),
            clock,
            new List<string>(),
            AdaptiveReorgBaselineEngine.DefaultAdaptiveReorgBaselineEngine());
    }
}
