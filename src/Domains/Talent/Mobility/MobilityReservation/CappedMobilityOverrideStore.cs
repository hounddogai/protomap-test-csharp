namespace Acme.Workforce.Talent.Mobility.MobilityReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped mobility overrides by code.</summary>
public interface ICappedMobilityOverrideStore
{
    CappedMobilityOverride? Find(string code);
    void Save(CappedMobilityOverride item);
    bool Remove(string code);
    List<CappedMobilityOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedMobilityOverrideStore(Dictionary<string, CappedMobilityOverride> items, List<string> order) : ICappedMobilityOverrideStore
{
    public Dictionary<string, CappedMobilityOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedMobilityOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedMobilityOverride item)
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

    public List<CappedMobilityOverride> List()
    {
        var outValue = new List<CappedMobilityOverride>();
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
public sealed class CachedCappedMobilityOverrideStore(MemoryCappedMobilityOverrideStore inner, CappedMobilityOverrideCache cache, int hits, int misses) : ICappedMobilityOverrideStore
{
    public MemoryCappedMobilityOverrideStore Inner { get; set; } = inner;
    public CappedMobilityOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedMobilityOverride? Find(string code)
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

    public void Save(CappedMobilityOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedMobilityOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped mobility overrides. Empty fields match all.</summary>
public sealed record CappedMobilityOverrideQuery(string Team, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(CappedMobilityOverride item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedMobilityOverride> Apply(List<CappedMobilityOverride> items)
    {
        var matched = Collections.Filter(items, (CappedMobilityOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped mobility override store.</summary>
public static class CappedMobilityOverrideStore
{
    public static ICappedMobilityOverrideStore NewMemoryCappedMobilityOverrideStore()
    {
        return new MemoryCappedMobilityOverrideStore(
            new Dictionary<string, CappedMobilityOverride>(),
            new List<string>());
    }

    public static ICappedMobilityOverrideStore NewCachedCappedMobilityOverrideStore(MemoryCappedMobilityOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedCappedMobilityOverrideStore(
            inner,
            CappedMobilityOverrideCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<CappedMobilityOverride> FindCappedMobilityOverrides(ICappedMobilityOverrideStore store, CappedMobilityOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
