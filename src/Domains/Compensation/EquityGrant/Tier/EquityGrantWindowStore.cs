namespace Acme.Workforce.Compensation.EquityGrant.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant windows by code.</summary>
public interface IEquityGrantWindowStore
{
    EquityGrantWindow? Find(string code);
    void Save(EquityGrantWindow item);
    bool Remove(string code);
    List<EquityGrantWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantWindowStore(Dictionary<string, EquityGrantWindow> items, List<string> order) : IEquityGrantWindowStore
{
    public Dictionary<string, EquityGrantWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantWindow item)
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

    public List<EquityGrantWindow> List()
    {
        var outValue = new List<EquityGrantWindow>();
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
public sealed class CachedEquityGrantWindowStore(MemoryEquityGrantWindowStore inner, EquityGrantWindowCache cache, int hits, int misses) : IEquityGrantWindowStore
{
    public MemoryEquityGrantWindowStore Inner { get; set; } = inner;
    public EquityGrantWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantWindow? Find(string code)
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

    public void Save(EquityGrantWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant windows. Empty fields match all.</summary>
public sealed record EquityGrantWindowQuery(string Label, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantWindow item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantWindow> Apply(List<EquityGrantWindow> items)
    {
        var matched = Collections.Filter(items, (EquityGrantWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant window store.</summary>
public static class EquityGrantWindowStore
{
    public static IEquityGrantWindowStore NewMemoryEquityGrantWindowStore()
    {
        return new MemoryEquityGrantWindowStore(new Dictionary<string, EquityGrantWindow>(), new List<string>());
    }

    public static IEquityGrantWindowStore NewCachedEquityGrantWindowStore(MemoryEquityGrantWindowStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantWindowStore(inner, EquityGrantWindowCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<EquityGrantWindow> FindEquityGrantWindows(IEquityGrantWindowStore store, EquityGrantWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
