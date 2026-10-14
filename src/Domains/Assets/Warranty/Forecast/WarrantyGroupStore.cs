namespace Acme.Workforce.Assets.Warranty.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty groups by code.</summary>
public interface IWarrantyGroupStore
{
    WarrantyGroup? Find(string code);
    void Save(WarrantyGroup item);
    bool Remove(string code);
    List<WarrantyGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyGroupStore(Dictionary<string, WarrantyGroup> items, List<string> order) : IWarrantyGroupStore
{
    public Dictionary<string, WarrantyGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyGroup item)
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

    public List<WarrantyGroup> List()
    {
        var outValue = new List<WarrantyGroup>();
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
public sealed class CachedWarrantyGroupStore(MemoryWarrantyGroupStore inner, WarrantyGroupCache cache, int hits, int misses) : IWarrantyGroupStore
{
    public MemoryWarrantyGroupStore Inner { get; set; } = inner;
    public WarrantyGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyGroup? Find(string code)
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

    public void Save(WarrantyGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty groups. Empty fields match all.</summary>
public sealed record WarrantyGroupQuery(string Name, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyGroup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyGroup> Apply(List<WarrantyGroup> items)
    {
        var matched = Collections.Filter(items, (WarrantyGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty group store.</summary>
public static class WarrantyGroupStore
{
    public static IWarrantyGroupStore NewMemoryWarrantyGroupStore()
    {
        return new MemoryWarrantyGroupStore(new Dictionary<string, WarrantyGroup>(), new List<string>());
    }

    public static IWarrantyGroupStore NewCachedWarrantyGroupStore(MemoryWarrantyGroupStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyGroupStore(inner, WarrantyGroupCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<WarrantyGroup> FindWarrantyGroups(IWarrantyGroupStore store, WarrantyGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
