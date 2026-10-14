namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary garnishment mappings by code.</summary>
public interface IPrimaryGarnishmentMappingStore
{
    PrimaryGarnishmentMapping? Find(string code);
    void Save(PrimaryGarnishmentMapping item);
    bool Remove(string code);
    List<PrimaryGarnishmentMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryGarnishmentMappingStore(Dictionary<string, PrimaryGarnishmentMapping> items, List<string> order) : IPrimaryGarnishmentMappingStore
{
    public Dictionary<string, PrimaryGarnishmentMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryGarnishmentMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryGarnishmentMapping item)
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

    public List<PrimaryGarnishmentMapping> List()
    {
        var outValue = new List<PrimaryGarnishmentMapping>();
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
public sealed class CachedPrimaryGarnishmentMappingStore(MemoryPrimaryGarnishmentMappingStore inner, PrimaryGarnishmentMappingCache cache, int hits, int misses) : IPrimaryGarnishmentMappingStore
{
    public MemoryPrimaryGarnishmentMappingStore Inner { get; set; } = inner;
    public PrimaryGarnishmentMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryGarnishmentMapping? Find(string code)
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

    public void Save(PrimaryGarnishmentMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryGarnishmentMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary garnishment mappings. Empty fields match all.</summary>
public sealed record PrimaryGarnishmentMappingQuery(string Name, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryGarnishmentMapping item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryGarnishmentMapping> Apply(List<PrimaryGarnishmentMapping> items)
    {
        var matched = Collections.Filter(items, (PrimaryGarnishmentMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary garnishment mapping store.</summary>
public static class PrimaryGarnishmentMappingStore
{
    public static IPrimaryGarnishmentMappingStore NewMemoryPrimaryGarnishmentMappingStore()
    {
        return new MemoryPrimaryGarnishmentMappingStore(
            new Dictionary<string, PrimaryGarnishmentMapping>(),
            new List<string>());
    }

    public static IPrimaryGarnishmentMappingStore NewCachedPrimaryGarnishmentMappingStore(MemoryPrimaryGarnishmentMappingStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryGarnishmentMappingStore(
            inner,
            PrimaryGarnishmentMappingCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryGarnishmentMapping> FindPrimaryGarnishmentMappings(IPrimaryGarnishmentMappingStore store, PrimaryGarnishmentMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
