namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending reorg baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingReorgBaselineCoordinator(IPendingReorgBaselineStore Store, PendingReorgBaselineValidator Validator, IClock Clock, List<string> Journal, PendingReorgBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending reorg baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingReorgBaselineStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending reorg baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingReorgBaselineStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending reorg baseline {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of pending reorg baseline workflow.</summary>
public static class PendingReorgBaselineWorkflow
{
    public static PendingReorgBaselineCoordinator NewPendingReorgBaselineCoordinator(IPendingReorgBaselineStore store, IClock clock)
    {
        return new PendingReorgBaselineCoordinator(
            store,
            PendingReorgBaselineRules.DefaultPendingReorgBaselineValidator(),
            clock,
            new List<string>(),
            PendingReorgBaselineEngine.DefaultPendingReorgBaselineEngine());
    }
}
