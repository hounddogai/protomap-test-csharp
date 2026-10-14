namespace Acme.Workforce.Workplace.Occupancy.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy policies by code.</summary>
public interface IOccupancyPolicyStore
{
    OccupancyPolicy? Find(string code);
    void Save(OccupancyPolicy item);
    bool Remove(string code);
    List<OccupancyPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyPolicyStore(Dictionary<string, OccupancyPolicy> items, List<string> order) : IOccupancyPolicyStore
{
    public Dictionary<string, OccupancyPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyPolicy item)
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

    public List<OccupancyPolicy> List()
    {
        var outValue = new List<OccupancyPolicy>();
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
public sealed class CachedOccupancyPolicyStore(MemoryOccupancyPolicyStore inner, OccupancyPolicyCache cache, int hits, int misses) : IOccupancyPolicyStore
{
    public MemoryOccupancyPolicyStore Inner { get; set; } = inner;
    public OccupancyPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyPolicy? Find(string code)
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

    public void Save(OccupancyPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy policies. Empty fields match all.</summary>
public sealed record OccupancyPolicyQuery(string Title, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyPolicy item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OccupancyPolicy> Apply(List<OccupancyPolicy> items)
    {
        var matched = Collections.Filter(items, (OccupancyPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy policy store.</summary>
public static class OccupancyPolicyStore
{
    public static IOccupancyPolicyStore NewMemoryOccupancyPolicyStore()
    {
        return new MemoryOccupancyPolicyStore(new Dictionary<string, OccupancyPolicy>(), new List<string>());
    }

    public static IOccupancyPolicyStore NewCachedOccupancyPolicyStore(MemoryOccupancyPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyPolicyStore(inner, OccupancyPolicyCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<OccupancyPolicy> FindOccupancyPolicies(IOccupancyPolicyStore store, OccupancyPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
