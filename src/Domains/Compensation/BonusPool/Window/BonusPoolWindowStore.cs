namespace Acme.Workforce.Compensation.BonusPool.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool windows by code.</summary>
public interface IBonusPoolWindowStore
{
    BonusPoolWindow? Find(string code);
    void Save(BonusPoolWindow item);
    bool Remove(string code);
    List<BonusPoolWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolWindowStore(Dictionary<string, BonusPoolWindow> items, List<string> order) : IBonusPoolWindowStore
{
    public Dictionary<string, BonusPoolWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolWindow item)
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

    public List<BonusPoolWindow> List()
    {
        var outValue = new List<BonusPoolWindow>();
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
public sealed class CachedBonusPoolWindowStore(MemoryBonusPoolWindowStore inner, BonusPoolWindowCache cache, int hits, int misses) : IBonusPoolWindowStore
{
    public MemoryBonusPoolWindowStore Inner { get; set; } = inner;
    public BonusPoolWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolWindow? Find(string code)
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

    public void Save(BonusPoolWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool windows. Empty fields match all.</summary>
public sealed record BonusPoolWindowQuery(string Name, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolWindow item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolWindow> Apply(List<BonusPoolWindow> items)
    {
        var matched = Collections.Filter(items, (BonusPoolWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool window store.</summary>
public static class BonusPoolWindowStore
{
    public static IBonusPoolWindowStore NewMemoryBonusPoolWindowStore()
    {
        return new MemoryBonusPoolWindowStore(new Dictionary<string, BonusPoolWindow>(), new List<string>());
    }

    public static IBonusPoolWindowStore NewCachedBonusPoolWindowStore(MemoryBonusPoolWindowStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolWindowStore(inner, BonusPoolWindowCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BonusPoolWindow> FindBonusPoolWindows(IBonusPoolWindowStore store, BonusPoolWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
