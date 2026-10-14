namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves fallback rate cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record FallbackRateCycleCoordinator(IFallbackRateCycleStore Store, FallbackRateCycleValidator Validator, IClock Clock, List<string> Journal, FallbackRateCycleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback rate cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackRateCycleStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback rate cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackRateCycleStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback rate cycle {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of fallback rate cycle workflow.</summary>
public static class FallbackRateCycleWorkflow
{
    public static FallbackRateCycleCoordinator NewFallbackRateCycleCoordinator(IFallbackRateCycleStore store, IClock clock, Registry flags)
    {
        return new FallbackRateCycleCoordinator(
            store,
            FallbackRateCycleRules.DefaultFallbackRateCycleValidator(),
            clock,
            new List<string>(),
            FallbackRateCycleEngine.DefaultFallbackRateCycleEngine(flags));
    }
}
