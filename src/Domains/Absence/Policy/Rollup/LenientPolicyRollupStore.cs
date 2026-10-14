namespace Acme.Workforce.Absence.Policy.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient policy rollups by code.</summary>
public interface ILenientPolicyRollupStore
{
    LenientPolicyRollup? Find(string code);
    void Save(LenientPolicyRollup item);
    bool Remove(string code);
    List<LenientPolicyRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientPolicyRollupStore(Dictionary<string, LenientPolicyRollup> items, List<string> order) : ILenientPolicyRollupStore
{
    public Dictionary<string, LenientPolicyRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientPolicyRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientPolicyRollup item)
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

    public List<LenientPolicyRollup> List()
    {
        var outValue = new List<LenientPolicyRollup>();
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
public sealed class CachedLenientPolicyRollupStore(MemoryLenientPolicyRollupStore inner, LenientPolicyRollupCache cache, int hits, int misses) : ILenientPolicyRollupStore
{
    public MemoryLenientPolicyRollupStore Inner { get; set; } = inner;
    public LenientPolicyRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientPolicyRollup? Find(string code)
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

    public void Save(LenientPolicyRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientPolicyRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient policy rollups. Empty fields match all.</summary>
public sealed record LenientPolicyRollupQuery(string Department, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(LenientPolicyRollup item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientPolicyRollup> Apply(List<LenientPolicyRollup> items)
    {
        var matched = Collections.Filter(items, (LenientPolicyRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient policy rollup store.</summary>
public static class LenientPolicyRollupStore
{
    public static ILenientPolicyRollupStore NewMemoryLenientPolicyRollupStore()
    {
        return new MemoryLenientPolicyRollupStore(new Dictionary<string, LenientPolicyRollup>(), new List<string>());
    }

    public static ILenientPolicyRollupStore NewCachedLenientPolicyRollupStore(MemoryLenientPolicyRollupStore inner, int ttl, IClock clock)
    {
        return new CachedLenientPolicyRollupStore(inner, LenientPolicyRollupCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LenientPolicyRollup> FindLenientPolicyRollups(ILenientPolicyRollupStore store, LenientPolicyRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
