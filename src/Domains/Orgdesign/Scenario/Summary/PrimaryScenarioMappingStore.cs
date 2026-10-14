namespace Acme.Workforce.Orgdesign.Scenario.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary scenario mappings by code.</summary>
public interface IPrimaryScenarioMappingStore
{
    PrimaryScenarioMapping? Find(string code);
    void Save(PrimaryScenarioMapping item);
    bool Remove(string code);
    List<PrimaryScenarioMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryScenarioMappingStore(Dictionary<string, PrimaryScenarioMapping> items, List<string> order) : IPrimaryScenarioMappingStore
{
    public Dictionary<string, PrimaryScenarioMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryScenarioMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryScenarioMapping item)
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

    public List<PrimaryScenarioMapping> List()
    {
        var outValue = new List<PrimaryScenarioMapping>();
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
public sealed class CachedPrimaryScenarioMappingStore(MemoryPrimaryScenarioMappingStore inner, PrimaryScenarioMappingCache cache, int hits, int misses) : IPrimaryScenarioMappingStore
{
    public MemoryPrimaryScenarioMappingStore Inner { get; set; } = inner;
    public PrimaryScenarioMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryScenarioMapping? Find(string code)
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

    public void Save(PrimaryScenarioMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryScenarioMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary scenario mappings. Empty fields match all.</summary>
public sealed record PrimaryScenarioMappingQuery(string Unit, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryScenarioMapping item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<PrimaryScenarioMapping> Apply(List<PrimaryScenarioMapping> items)
    {
        var matched = Collections.Filter(items, (PrimaryScenarioMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary scenario mapping store.</summary>
public static class PrimaryScenarioMappingStore
{
    public static IPrimaryScenarioMappingStore NewMemoryPrimaryScenarioMappingStore()
    {
        return new MemoryPrimaryScenarioMappingStore(
            new Dictionary<string, PrimaryScenarioMapping>(),
            new List<string>());
    }

    public static IPrimaryScenarioMappingStore NewCachedPrimaryScenarioMappingStore(MemoryPrimaryScenarioMappingStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryScenarioMappingStore(
            inner,
            PrimaryScenarioMappingCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryScenarioMapping> FindPrimaryScenarioMappings(IPrimaryScenarioMappingStore store, PrimaryScenarioMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
