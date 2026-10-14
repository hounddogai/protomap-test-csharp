namespace Acme.Workforce.Workplace.Occupancy.Estimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default occupancy estimates through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultOccupancyEstimateCoordinator(IDefaultOccupancyEstimateStore Store, DefaultOccupancyEstimateValidator Validator, IClock Clock, List<string> Journal, DefaultOccupancyEstimateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default occupancy estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultOccupancyEstimateStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default occupancy estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultOccupancyEstimateStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default occupancy estimate {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of default occupancy estimate workflow.</summary>
public static class DefaultOccupancyEstimateWorkflow
{
    public static DefaultOccupancyEstimateCoordinator NewDefaultOccupancyEstimateCoordinator(IDefaultOccupancyEstimateStore store, IClock clock)
    {
        return new DefaultOccupancyEstimateCoordinator(
            store,
            DefaultOccupancyEstimateRules.DefaultDefaultOccupancyEstimateValidator(),
            clock,
            new List<string>(),
            DefaultOccupancyEstimateEngine.DefaultDefaultOccupancyEstimateEngine());
    }
}
