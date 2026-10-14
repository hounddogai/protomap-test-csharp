namespace Acme.Workforce.Assets.Warranty.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty targets by code.</summary>
public interface IWarrantyTargetStore
{
    WarrantyTarget? Find(string code);
    void Save(WarrantyTarget item);
    bool Remove(string code);
    List<WarrantyTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyTargetStore(Dictionary<string, WarrantyTarget> items, List<string> order) : IWarrantyTargetStore
{
    public Dictionary<string, WarrantyTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyTarget item)
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

    public List<WarrantyTarget> List()
    {
        var outValue = new List<WarrantyTarget>();
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
public sealed class CachedWarrantyTargetStore(MemoryWarrantyTargetStore inner, WarrantyTargetCache cache, int hits, int misses) : IWarrantyTargetStore
{
    public MemoryWarrantyTargetStore Inner { get; set; } = inner;
    public WarrantyTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyTarget? Find(string code)
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

    public void Save(WarrantyTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty targets. Empty fields match all.</summary>
public sealed record WarrantyTargetQuery(string Category, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyTarget item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyTarget> Apply(List<WarrantyTarget> items)
    {
        var matched = Collections.Filter(items, (WarrantyTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty target store.</summary>
public static class WarrantyTargetStore
{
    public static IWarrantyTargetStore NewMemoryWarrantyTargetStore()
    {
        return new MemoryWarrantyTargetStore(new Dictionary<string, WarrantyTarget>(), new List<string>());
    }

    public static IWarrantyTargetStore NewCachedWarrantyTargetStore(MemoryWarrantyTargetStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyTargetStore(inner, WarrantyTargetCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<WarrantyTarget> FindWarrantyTargets(IWarrantyTargetStore store, WarrantyTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
