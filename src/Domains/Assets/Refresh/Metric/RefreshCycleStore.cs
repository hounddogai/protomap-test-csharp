namespace Acme.Workforce.Assets.Refresh.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh cycles by code.</summary>
public interface IRefreshCycleStore
{
    RefreshCycle? Find(string code);
    void Save(RefreshCycle item);
    bool Remove(string code);
    List<RefreshCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshCycleStore(Dictionary<string, RefreshCycle> items, List<string> order) : IRefreshCycleStore
{
    public Dictionary<string, RefreshCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshCycle item)
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

    public List<RefreshCycle> List()
    {
        var outValue = new List<RefreshCycle>();
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
public sealed class CachedRefreshCycleStore(MemoryRefreshCycleStore inner, RefreshCycleCache cache, int hits, int misses) : IRefreshCycleStore
{
    public MemoryRefreshCycleStore Inner { get; set; } = inner;
    public RefreshCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshCycle? Find(string code)
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

    public void Save(RefreshCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh cycles. Empty fields match all.</summary>
public sealed record RefreshCycleQuery(string Channel, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshCycle item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RefreshCycle> Apply(List<RefreshCycle> items)
    {
        var matched = Collections.Filter(items, (RefreshCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh cycle store.</summary>
public static class RefreshCycleStore
{
    public static IRefreshCycleStore NewMemoryRefreshCycleStore()
    {
        return new MemoryRefreshCycleStore(new Dictionary<string, RefreshCycle>(), new List<string>());
    }

    public static IRefreshCycleStore NewCachedRefreshCycleStore(MemoryRefreshCycleStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshCycleStore(inner, RefreshCycleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RefreshCycle> FindRefreshCycles(IRefreshCycleStore store, RefreshCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
