namespace Acme.Workforce.Workplace.Visitor.VisitorOverride;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary visitor overrides by code.</summary>
public interface IPrimaryVisitorOverrideStore
{
    PrimaryVisitorOverride? Find(string code);
    void Save(PrimaryVisitorOverride item);
    bool Remove(string code);
    List<PrimaryVisitorOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryVisitorOverrideStore(Dictionary<string, PrimaryVisitorOverride> items, List<string> order) : IPrimaryVisitorOverrideStore
{
    public Dictionary<string, PrimaryVisitorOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryVisitorOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryVisitorOverride item)
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

    public List<PrimaryVisitorOverride> List()
    {
        var outValue = new List<PrimaryVisitorOverride>();
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
public sealed class CachedPrimaryVisitorOverrideStore(MemoryPrimaryVisitorOverrideStore inner, PrimaryVisitorOverrideCache cache, int hits, int misses) : IPrimaryVisitorOverrideStore
{
    public MemoryPrimaryVisitorOverrideStore Inner { get; set; } = inner;
    public PrimaryVisitorOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryVisitorOverride? Find(string code)
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

    public void Save(PrimaryVisitorOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryVisitorOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary visitor overrides. Empty fields match all.</summary>
public sealed record PrimaryVisitorOverrideQuery(string Category, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryVisitorOverride item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryVisitorOverride> Apply(List<PrimaryVisitorOverride> items)
    {
        var matched = Collections.Filter(items, (PrimaryVisitorOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary visitor override store.</summary>
public static class PrimaryVisitorOverrideStore
{
    public static IPrimaryVisitorOverrideStore NewMemoryPrimaryVisitorOverrideStore()
    {
        return new MemoryPrimaryVisitorOverrideStore(
            new Dictionary<string, PrimaryVisitorOverride>(),
            new List<string>());
    }

    public static IPrimaryVisitorOverrideStore NewCachedPrimaryVisitorOverrideStore(MemoryPrimaryVisitorOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryVisitorOverrideStore(
            inner,
            PrimaryVisitorOverrideCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryVisitorOverride> FindPrimaryVisitorOverrides(IPrimaryVisitorOverrideStore store, PrimaryVisitorOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
