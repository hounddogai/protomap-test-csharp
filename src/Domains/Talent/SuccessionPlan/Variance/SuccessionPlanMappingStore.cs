namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan mappings by code.</summary>
public interface ISuccessionPlanMappingStore
{
    SuccessionPlanMapping? Find(string code);
    void Save(SuccessionPlanMapping item);
    bool Remove(string code);
    List<SuccessionPlanMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanMappingStore(Dictionary<string, SuccessionPlanMapping> items, List<string> order) : ISuccessionPlanMappingStore
{
    public Dictionary<string, SuccessionPlanMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanMapping item)
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

    public List<SuccessionPlanMapping> List()
    {
        var outValue = new List<SuccessionPlanMapping>();
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
public sealed class CachedSuccessionPlanMappingStore(MemorySuccessionPlanMappingStore inner, SuccessionPlanMappingCache cache, int hits, int misses) : ISuccessionPlanMappingStore
{
    public MemorySuccessionPlanMappingStore Inner { get; set; } = inner;
    public SuccessionPlanMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanMapping? Find(string code)
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

    public void Save(SuccessionPlanMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan mappings. Empty fields match all.</summary>
public sealed record SuccessionPlanMappingQuery(string Title, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanMapping item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SuccessionPlanMapping> Apply(List<SuccessionPlanMapping> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan mapping store.</summary>
public static class SuccessionPlanMappingStore
{
    public static ISuccessionPlanMappingStore NewMemorySuccessionPlanMappingStore()
    {
        return new MemorySuccessionPlanMappingStore(
            new Dictionary<string, SuccessionPlanMapping>(),
            new List<string>());
    }

    public static ISuccessionPlanMappingStore NewCachedSuccessionPlanMappingStore(MemorySuccessionPlanMappingStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanMappingStore(inner, SuccessionPlanMappingCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SuccessionPlanMapping> FindSuccessionPlanMappings(ISuccessionPlanMappingStore store, SuccessionPlanMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
