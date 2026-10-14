namespace Acme.Workforce.Orgdesign.Position.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position policies by code.</summary>
public interface IPositionPolicyStore
{
    PositionPolicy? Find(string code);
    void Save(PositionPolicy item);
    bool Remove(string code);
    List<PositionPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionPolicyStore(Dictionary<string, PositionPolicy> items, List<string> order) : IPositionPolicyStore
{
    public Dictionary<string, PositionPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionPolicy item)
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

    public List<PositionPolicy> List()
    {
        var outValue = new List<PositionPolicy>();
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
public sealed class CachedPositionPolicyStore(MemoryPositionPolicyStore inner, PositionPolicyCache cache, int hits, int misses) : IPositionPolicyStore
{
    public MemoryPositionPolicyStore Inner { get; set; } = inner;
    public PositionPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionPolicy? Find(string code)
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

    public void Save(PositionPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position policies. Empty fields match all.</summary>
public sealed record PositionPolicyQuery(string Region, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(PositionPolicy item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<PositionPolicy> Apply(List<PositionPolicy> items)
    {
        var matched = Collections.Filter(items, (PositionPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position policy store.</summary>
public static class PositionPolicyStore
{
    public static IPositionPolicyStore NewMemoryPositionPolicyStore()
    {
        return new MemoryPositionPolicyStore(new Dictionary<string, PositionPolicy>(), new List<string>());
    }

    public static IPositionPolicyStore NewCachedPositionPolicyStore(MemoryPositionPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedPositionPolicyStore(inner, PositionPolicyCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PositionPolicy> FindPositionPolicies(IPositionPolicyStore store, PositionPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
