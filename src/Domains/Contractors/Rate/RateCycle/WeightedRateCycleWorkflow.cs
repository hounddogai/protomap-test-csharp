namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves weighted rate cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedRateCycleDesk(IWeightedRateCycleStore Store, WeightedRateCycleValidator Validator, IClock Clock, List<string> Journal, WeightedRateCycleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted rate cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedRateCycleStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted rate cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedRateCycleStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted rate cycle {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of weighted rate cycle workflow.</summary>
public static class WeightedRateCycleWorkflow
{
    public static WeightedRateCycleDesk NewWeightedRateCycleDesk(IWeightedRateCycleStore store, IClock clock, Registry flags)
    {
        return new WeightedRateCycleDesk(
            store,
            WeightedRateCycleRules.DefaultWeightedRateCycleValidator(),
            clock,
            new List<string>(),
            WeightedRateCycleEngine.DefaultWeightedRateCycleEngine(flags));
    }
}
