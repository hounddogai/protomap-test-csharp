namespace Acme.Workforce.Workplace.Occupancy.Estimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional occupancy tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalOccupancyTierManager(IRegionalOccupancyTierStore Store, RegionalOccupancyTierValidator Validator, IClock Clock, List<string> Journal, RegionalOccupancyTierEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional occupancy tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalOccupancyTierStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional occupancy tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalOccupancyTierStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional occupancy tier {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of regional occupancy tier workflow.</summary>
public static class RegionalOccupancyTierWorkflow
{
    public static RegionalOccupancyTierManager NewRegionalOccupancyTierManager(IRegionalOccupancyTierStore store, IClock clock)
    {
        return new RegionalOccupancyTierManager(
            store,
            RegionalOccupancyTierRules.DefaultRegionalOccupancyTierValidator(),
            clock,
            new List<string>(),
            RegionalOccupancyTierEngine.DefaultRegionalOccupancyTierEngine());
    }
}
