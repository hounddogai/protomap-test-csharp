namespace Acme.Workforce.Assets.Laptop.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted laptop estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedLaptopEstimateManager(IWeightedLaptopEstimateStore Store, WeightedLaptopEstimateValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted laptop estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedLaptopEstimateStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted laptop estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedLaptopEstimateStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of weighted laptop estimate workflow.</summary>
public static class WeightedLaptopEstimateWorkflow
{
    public static WeightedLaptopEstimateManager NewWeightedLaptopEstimateManager(IWeightedLaptopEstimateStore store, IClock clock)
    {
        return new WeightedLaptopEstimateManager(
            store,
            WeightedLaptopEstimateRules.DefaultWeightedLaptopEstimateValidator(),
            clock,
            new List<string>());
    }
}
