namespace Acme.Workforce.Workplace.Occupancy.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves legacy occupancy digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyOccupancyDigestDesk(ILegacyOccupancyDigestStore Store, LegacyOccupancyDigestValidator Validator, IClock Clock, List<string> Journal, LegacyOccupancyDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy occupancy digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyOccupancyDigestStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy occupancy digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyOccupancyDigestStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy occupancy digest {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of legacy occupancy digest workflow.</summary>
public static class LegacyOccupancyDigestWorkflow
{
    public static LegacyOccupancyDigestDesk NewLegacyOccupancyDigestDesk(ILegacyOccupancyDigestStore store, IClock clock, Registry flags)
    {
        return new LegacyOccupancyDigestDesk(
            store,
            LegacyOccupancyDigestRules.DefaultLegacyOccupancyDigestValidator(),
            clock,
            new List<string>(),
            LegacyOccupancyDigestEngine.DefaultLegacyOccupancyDigestEngine(flags));
    }
}
