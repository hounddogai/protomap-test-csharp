namespace Acme.Workforce.Assets.LicenseSeat.SeatCycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy license seat cycles by code.</summary>
public interface ILegacyLicenseSeatCycleStore
{
    LegacyLicenseSeatCycle? Find(string code);
    void Save(LegacyLicenseSeatCycle item);
    bool Remove(string code);
    List<LegacyLicenseSeatCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyLicenseSeatCycleStore(Dictionary<string, LegacyLicenseSeatCycle> items, List<string> order) : ILegacyLicenseSeatCycleStore
{
    public Dictionary<string, LegacyLicenseSeatCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyLicenseSeatCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyLicenseSeatCycle item)
    {
        if (!Items.ContainsKey(item.Code))
        {
            Order.Add(item.Code);
        }
        Items[item.Code] = item;
    }

    public bool Remove(string code)
    {
        if (!Items.ContainsKey(code))
        {
            return false;
        }
        Items.Remove(code);
        var kept = new List<string>();
        foreach (var existing in Order)
        {
            if (existing != code)
            {
                kept.Add(existing);
            }
        }
        Order = kept;
        return true;
    }

    public List<LegacyLicenseSeatCycle> List()
    {
        var outValue = new List<LegacyLicenseSeatCycle>();
        foreach (var key in Order)
        {
            if (!Items.TryGetValue(key, out var found))
            {
                continue;
            }
            outValue.Add(found);
        }
        return outValue;
    }
}

/// <summary>Serves reads from a short-lived cache in front of another store.</summary>
public sealed class CachedLegacyLicenseSeatCycleStore(MemoryLegacyLicenseSeatCycleStore inner, LegacyLicenseSeatCycleCache cache, int hits, int misses) : ILegacyLicenseSeatCycleStore
{
    public MemoryLegacyLicenseSeatCycleStore Inner { get; set; } = inner;
    public LegacyLicenseSeatCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyLicenseSeatCycle? Find(string code)
    {
        if (Cache.Get(code) is not { } hit)
        {
            Misses += 1;
            if (Inner.Find(code) is not { } found)
            {
                return null;
            }
            Cache.Put(code, found);
            return found;
        }
        Hits += 1;
        return hit;
    }

    public void Save(LegacyLicenseSeatCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyLicenseSeatCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy license seat cycles. Empty fields match all.</summary>
public sealed record LegacyLicenseSeatCycleQuery(string Name, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyLicenseSeatCycle item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyLicenseSeatCycle> Apply(List<LegacyLicenseSeatCycle> items)
    {
        var matched = Collections.Filter(items, (LegacyLicenseSeatCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy license seat cycle store.</summary>
public static class LegacyLicenseSeatCycleStore
{
    public static ILegacyLicenseSeatCycleStore NewMemoryLegacyLicenseSeatCycleStore()
    {
        return new MemoryLegacyLicenseSeatCycleStore(
            new Dictionary<string, LegacyLicenseSeatCycle>(),
            new List<string>());
    }

    public static ILegacyLicenseSeatCycleStore NewCachedLegacyLicenseSeatCycleStore(MemoryLegacyLicenseSeatCycleStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyLicenseSeatCycleStore(
            inner,
            LegacyLicenseSeatCycleCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LegacyLicenseSeatCycle> FindLegacyLicenseSeatCycles(ILegacyLicenseSeatCycleStore store, LegacyLicenseSeatCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
