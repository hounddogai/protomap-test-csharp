namespace Acme.Workforce.Talent.SuccessionPlan.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan digests by code.</summary>
public interface ISuccessionPlanDigestStore
{
    SuccessionPlanDigest? Find(string code);
    void Save(SuccessionPlanDigest item);
    bool Remove(string code);
    List<SuccessionPlanDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanDigestStore(Dictionary<string, SuccessionPlanDigest> items, List<string> order) : ISuccessionPlanDigestStore
{
    public Dictionary<string, SuccessionPlanDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanDigest item)
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

    public List<SuccessionPlanDigest> List()
    {
        var outValue = new List<SuccessionPlanDigest>();
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
public sealed class CachedSuccessionPlanDigestStore(MemorySuccessionPlanDigestStore inner, SuccessionPlanDigestCache cache, int hits, int misses) : ISuccessionPlanDigestStore
{
    public MemorySuccessionPlanDigestStore Inner { get; set; } = inner;
    public SuccessionPlanDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanDigest? Find(string code)
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

    public void Save(SuccessionPlanDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan digests. Empty fields match all.</summary>
public sealed record SuccessionPlanDigestQuery(string Department, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanDigest item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SuccessionPlanDigest> Apply(List<SuccessionPlanDigest> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan digest store.</summary>
public static class SuccessionPlanDigestStore
{
    public static ISuccessionPlanDigestStore NewMemorySuccessionPlanDigestStore()
    {
        return new MemorySuccessionPlanDigestStore(
            new Dictionary<string, SuccessionPlanDigest>(),
            new List<string>());
    }

    public static ISuccessionPlanDigestStore NewCachedSuccessionPlanDigestStore(MemorySuccessionPlanDigestStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanDigestStore(inner, SuccessionPlanDigestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SuccessionPlanDigest> FindSuccessionPlanDigests(ISuccessionPlanDigestStore store, SuccessionPlanDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
