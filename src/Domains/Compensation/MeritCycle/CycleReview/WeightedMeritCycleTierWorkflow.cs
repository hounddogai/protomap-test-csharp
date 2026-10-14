namespace Acme.Workforce.Compensation.MeritCycle.CycleReview;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted merit cycle tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedMeritCycleTierWorkflow(IWeightedMeritCycleTierStore Store, WeightedMeritCycleTierValidator Validator, IClock Clock, List<string> Journal, WeightedMeritCycleTierEngine Engine)
{
    public static WeightedMeritCycleTierWorkflow NewWeightedMeritCycleTierWorkflow(IWeightedMeritCycleTierStore store, IClock clock)
    {
        return new WeightedMeritCycleTierWorkflow(
            store,
            WeightedMeritCycleTierRules.DefaultWeightedMeritCycleTierValidator(),
            clock,
            new List<string>(),
            WeightedMeritCycleTierEngine.DefaultWeightedMeritCycleTierEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted merit cycle tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedMeritCycleTierStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted merit cycle tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedMeritCycleTierStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted merit cycle tier {code} not found");
        }
        var result = Engine.Compute(item);
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
