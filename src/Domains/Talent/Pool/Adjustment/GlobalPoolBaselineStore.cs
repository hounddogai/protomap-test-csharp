namespace Acme.Workforce.Talent.Pool.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global pool baselines by code.</summary>
public interface IGlobalPoolBaselineStore
{
    GlobalPoolBaseline? Find(string code);
    void Save(GlobalPoolBaseline item);
    bool Remove(string code);
    List<GlobalPoolBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalPoolBaselineStore(Dictionary<string, GlobalPoolBaseline> items, List<string> order) : IGlobalPoolBaselineStore
{
    public Dictionary<string, GlobalPoolBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalPoolBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalPoolBaseline item)
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

    public List<GlobalPoolBaseline> List()
    {
        var outValue = new List<GlobalPoolBaseline>();
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
public sealed class CachedGlobalPoolBaselineStore(MemoryGlobalPoolBaselineStore inner, GlobalPoolBaselineCache cache, int hits, int misses) : IGlobalPoolBaselineStore
{
    public MemoryGlobalPoolBaselineStore Inner { get; set; } = inner;
    public GlobalPoolBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalPoolBaseline? Find(string code)
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

    public void Save(GlobalPoolBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalPoolBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global pool baselines. Empty fields match all.</summary>
public sealed record GlobalPoolBaselineQuery(string Name, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalPoolBaseline item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalPoolBaseline> Apply(List<GlobalPoolBaseline> items)
    {
        var matched = Collections.Filter(items, (GlobalPoolBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global pool baseline store.</summary>
public static class GlobalPoolBaselineStore
{
    public static IGlobalPoolBaselineStore NewMemoryGlobalPoolBaselineStore()
    {
        return new MemoryGlobalPoolBaselineStore(new Dictionary<string, GlobalPoolBaseline>(), new List<string>());
    }

    public static IGlobalPoolBaselineStore NewCachedGlobalPoolBaselineStore(MemoryGlobalPoolBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalPoolBaselineStore(inner, GlobalPoolBaselineCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<GlobalPoolBaseline> FindGlobalPoolBaselines(IGlobalPoolBaselineStore store, GlobalPoolBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
