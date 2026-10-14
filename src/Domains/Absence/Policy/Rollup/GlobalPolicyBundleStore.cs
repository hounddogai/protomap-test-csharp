namespace Acme.Workforce.Absence.Policy.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global policy bundles by code.</summary>
public interface IGlobalPolicyBundleStore
{
    GlobalPolicyBundle? Find(string code);
    void Save(GlobalPolicyBundle item);
    bool Remove(string code);
    List<GlobalPolicyBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalPolicyBundleStore(Dictionary<string, GlobalPolicyBundle> items, List<string> order) : IGlobalPolicyBundleStore
{
    public Dictionary<string, GlobalPolicyBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalPolicyBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalPolicyBundle item)
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

    public List<GlobalPolicyBundle> List()
    {
        var outValue = new List<GlobalPolicyBundle>();
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
public sealed class CachedGlobalPolicyBundleStore(MemoryGlobalPolicyBundleStore inner, GlobalPolicyBundleCache cache, int hits, int misses) : IGlobalPolicyBundleStore
{
    public MemoryGlobalPolicyBundleStore Inner { get; set; } = inner;
    public GlobalPolicyBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalPolicyBundle? Find(string code)
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

    public void Save(GlobalPolicyBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalPolicyBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global policy bundles. Empty fields match all.</summary>
public sealed record GlobalPolicyBundleQuery(string Unit, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalPolicyBundle item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalPolicyBundle> Apply(List<GlobalPolicyBundle> items)
    {
        var matched = Collections.Filter(items, (GlobalPolicyBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global policy bundle store.</summary>
public static class GlobalPolicyBundleStore
{
    public static IGlobalPolicyBundleStore NewMemoryGlobalPolicyBundleStore()
    {
        return new MemoryGlobalPolicyBundleStore(new Dictionary<string, GlobalPolicyBundle>(), new List<string>());
    }

    public static IGlobalPolicyBundleStore NewCachedGlobalPolicyBundleStore(MemoryGlobalPolicyBundleStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalPolicyBundleStore(inner, GlobalPolicyBundleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<GlobalPolicyBundle> FindGlobalPolicyBundles(IGlobalPolicyBundleStore store, GlobalPolicyBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
