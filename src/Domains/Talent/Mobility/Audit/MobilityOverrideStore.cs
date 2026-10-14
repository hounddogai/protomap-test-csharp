namespace Acme.Workforce.Talent.Mobility.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility overrides by code.</summary>
public interface IMobilityOverrideStore
{
    MobilityOverride? Find(string code);
    void Save(MobilityOverride item);
    bool Remove(string code);
    List<MobilityOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilityOverrideStore(Dictionary<string, MobilityOverride> items, List<string> order) : IMobilityOverrideStore
{
    public Dictionary<string, MobilityOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilityOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilityOverride item)
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

    public List<MobilityOverride> List()
    {
        var outValue = new List<MobilityOverride>();
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
public sealed class CachedMobilityOverrideStore(MemoryMobilityOverrideStore inner, MobilityOverrideCache cache, int hits, int misses) : IMobilityOverrideStore
{
    public MemoryMobilityOverrideStore Inner { get; set; } = inner;
    public MobilityOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilityOverride? Find(string code)
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

    public void Save(MobilityOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilityOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility overrides. Empty fields match all.</summary>
public sealed record MobilityOverrideQuery(string Locale, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(MobilityOverride item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MobilityOverride> Apply(List<MobilityOverride> items)
    {
        var matched = Collections.Filter(items, (MobilityOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility override store.</summary>
public static class MobilityOverrideStore
{
    public static IMobilityOverrideStore NewMemoryMobilityOverrideStore()
    {
        return new MemoryMobilityOverrideStore(new Dictionary<string, MobilityOverride>(), new List<string>());
    }

    public static IMobilityOverrideStore NewCachedMobilityOverrideStore(MemoryMobilityOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedMobilityOverrideStore(inner, MobilityOverrideCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<MobilityOverride> FindMobilityOverrides(IMobilityOverrideStore store, MobilityOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
