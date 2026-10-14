namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty cycles by code.</summary>
public interface IWarrantyCycleStore
{
    WarrantyCycle? Find(string code);
    void Save(WarrantyCycle item);
    bool Remove(string code);
    List<WarrantyCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyCycleStore(Dictionary<string, WarrantyCycle> items, List<string> order) : IWarrantyCycleStore
{
    public Dictionary<string, WarrantyCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyCycle item)
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

    public List<WarrantyCycle> List()
    {
        var outValue = new List<WarrantyCycle>();
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
public sealed class CachedWarrantyCycleStore(MemoryWarrantyCycleStore inner, WarrantyCycleCache cache, int hits, int misses) : IWarrantyCycleStore
{
    public MemoryWarrantyCycleStore Inner { get; set; } = inner;
    public WarrantyCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyCycle? Find(string code)
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

    public void Save(WarrantyCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty cycles. Empty fields match all.</summary>
public sealed record WarrantyCycleQuery(string Team, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyCycle item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<WarrantyCycle> Apply(List<WarrantyCycle> items)
    {
        var matched = Collections.Filter(items, (WarrantyCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty cycle store.</summary>
public static class WarrantyCycleStore
{
    public static IWarrantyCycleStore NewMemoryWarrantyCycleStore()
    {
        return new MemoryWarrantyCycleStore(new Dictionary<string, WarrantyCycle>(), new List<string>());
    }

    public static IWarrantyCycleStore NewCachedWarrantyCycleStore(MemoryWarrantyCycleStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyCycleStore(inner, WarrantyCycleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<WarrantyCycle> FindWarrantyCycles(IWarrantyCycleStore store, WarrantyCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
