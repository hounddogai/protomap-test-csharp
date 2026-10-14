namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending offboarding cycles by code.</summary>
public interface IPendingOffboardingCycleStore
{
    PendingOffboardingCycle? Find(string code);
    void Save(PendingOffboardingCycle item);
    bool Remove(string code);
    List<PendingOffboardingCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingOffboardingCycleStore(Dictionary<string, PendingOffboardingCycle> items, List<string> order) : IPendingOffboardingCycleStore
{
    public Dictionary<string, PendingOffboardingCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingOffboardingCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingOffboardingCycle item)
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

    public List<PendingOffboardingCycle> List()
    {
        var outValue = new List<PendingOffboardingCycle>();
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
public sealed class CachedPendingOffboardingCycleStore(MemoryPendingOffboardingCycleStore inner, PendingOffboardingCycleCache cache, int hits, int misses) : IPendingOffboardingCycleStore
{
    public MemoryPendingOffboardingCycleStore Inner { get; set; } = inner;
    public PendingOffboardingCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingOffboardingCycle? Find(string code)
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

    public void Save(PendingOffboardingCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingOffboardingCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending offboarding cycles. Empty fields match all.</summary>
public sealed record PendingOffboardingCycleQuery(string Channel, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PendingOffboardingCycle item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingOffboardingCycle> Apply(List<PendingOffboardingCycle> items)
    {
        var matched = Collections.Filter(items, (PendingOffboardingCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending offboarding cycle store.</summary>
public static class PendingOffboardingCycleStore
{
    public static IPendingOffboardingCycleStore NewMemoryPendingOffboardingCycleStore()
    {
        return new MemoryPendingOffboardingCycleStore(
            new Dictionary<string, PendingOffboardingCycle>(),
            new List<string>());
    }

    public static IPendingOffboardingCycleStore NewCachedPendingOffboardingCycleStore(MemoryPendingOffboardingCycleStore inner, int ttl, IClock clock)
    {
        return new CachedPendingOffboardingCycleStore(
            inner,
            PendingOffboardingCycleCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PendingOffboardingCycle> FindPendingOffboardingCycles(IPendingOffboardingCycleStore store, PendingOffboardingCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
