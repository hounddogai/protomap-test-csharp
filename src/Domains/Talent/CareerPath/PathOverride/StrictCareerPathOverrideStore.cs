namespace Acme.Workforce.Talent.CareerPath.PathOverride;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict career path overrides by code.</summary>
public interface IStrictCareerPathOverrideStore
{
    StrictCareerPathOverride? Find(string code);
    void Save(StrictCareerPathOverride item);
    bool Remove(string code);
    List<StrictCareerPathOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictCareerPathOverrideStore(Dictionary<string, StrictCareerPathOverride> items, List<string> order) : IStrictCareerPathOverrideStore
{
    public Dictionary<string, StrictCareerPathOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictCareerPathOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictCareerPathOverride item)
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

    public List<StrictCareerPathOverride> List()
    {
        var outValue = new List<StrictCareerPathOverride>();
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
public sealed class CachedStrictCareerPathOverrideStore(MemoryStrictCareerPathOverrideStore inner, StrictCareerPathOverrideCache cache, int hits, int misses) : IStrictCareerPathOverrideStore
{
    public MemoryStrictCareerPathOverrideStore Inner { get; set; } = inner;
    public StrictCareerPathOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictCareerPathOverride? Find(string code)
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

    public void Save(StrictCareerPathOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictCareerPathOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict career path overrides. Empty fields match all.</summary>
public sealed record StrictCareerPathOverrideQuery(string Unit, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(StrictCareerPathOverride item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictCareerPathOverride> Apply(List<StrictCareerPathOverride> items)
    {
        var matched = Collections.Filter(items, (StrictCareerPathOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict career path override store.</summary>
public static class StrictCareerPathOverrideStore
{
    public static IStrictCareerPathOverrideStore NewMemoryStrictCareerPathOverrideStore()
    {
        return new MemoryStrictCareerPathOverrideStore(
            new Dictionary<string, StrictCareerPathOverride>(),
            new List<string>());
    }

    public static IStrictCareerPathOverrideStore NewCachedStrictCareerPathOverrideStore(MemoryStrictCareerPathOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedStrictCareerPathOverrideStore(
            inner,
            StrictCareerPathOverrideCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<StrictCareerPathOverride> FindStrictCareerPathOverrides(IStrictCareerPathOverrideStore store, StrictCareerPathOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
