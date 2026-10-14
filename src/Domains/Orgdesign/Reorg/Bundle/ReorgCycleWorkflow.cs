namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves reorg cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReorgCycleManager(IReorgCycleStore Store, ReorgCycleValidator Validator, IClock Clock, List<string> Journal, ReorgCycleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReorgCycleStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReorgCycleStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg cycle {code} not found");
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

/// <summary>Functions of reorg cycle workflow.</summary>
public static class ReorgCycleWorkflow
{
    public static ReorgCycleManager NewReorgCycleManager(IReorgCycleStore store, IClock clock)
    {
        return new ReorgCycleManager(
            store,
            ReorgCycleRules.DefaultReorgCycleValidator(),
            clock,
            new List<string>(),
            ReorgCycleEngine.DefaultReorgCycleEngine());
    }
}
