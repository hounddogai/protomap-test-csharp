namespace Acme.Workforce.Absence.Balance.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance windows by code.</summary>
public interface IBalanceWindowStore
{
    BalanceWindow? Find(string code);
    void Save(BalanceWindow item);
    bool Remove(string code);
    List<BalanceWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalanceWindowStore(Dictionary<string, BalanceWindow> items, List<string> order) : IBalanceWindowStore
{
    public Dictionary<string, BalanceWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalanceWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalanceWindow item)
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

    public List<BalanceWindow> List()
    {
        var outValue = new List<BalanceWindow>();
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
public sealed class CachedBalanceWindowStore(MemoryBalanceWindowStore inner, BalanceWindowCache cache, int hits, int misses) : IBalanceWindowStore
{
    public MemoryBalanceWindowStore Inner { get; set; } = inner;
    public BalanceWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalanceWindow? Find(string code)
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

    public void Save(BalanceWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalanceWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance windows. Empty fields match all.</summary>
public sealed record BalanceWindowQuery(string Name, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(BalanceWindow item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BalanceWindow> Apply(List<BalanceWindow> items)
    {
        var matched = Collections.Filter(items, (BalanceWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance window store.</summary>
public static class BalanceWindowStore
{
    public static IBalanceWindowStore NewMemoryBalanceWindowStore()
    {
        return new MemoryBalanceWindowStore(new Dictionary<string, BalanceWindow>(), new List<string>());
    }

    public static IBalanceWindowStore NewCachedBalanceWindowStore(MemoryBalanceWindowStore inner, int ttl, IClock clock)
    {
        return new CachedBalanceWindowStore(inner, BalanceWindowCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BalanceWindow> FindBalanceWindows(IBalanceWindowStore store, BalanceWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
