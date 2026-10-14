namespace Acme.Workforce.Compliance.PolicyAck.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered policy ack overrides by code.</summary>
public interface ITieredPolicyAckOverrideStore
{
    TieredPolicyAckOverride? Find(string code);
    void Save(TieredPolicyAckOverride item);
    bool Remove(string code);
    List<TieredPolicyAckOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredPolicyAckOverrideStore(Dictionary<string, TieredPolicyAckOverride> items, List<string> order) : ITieredPolicyAckOverrideStore
{
    public Dictionary<string, TieredPolicyAckOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredPolicyAckOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredPolicyAckOverride item)
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

    public List<TieredPolicyAckOverride> List()
    {
        var outValue = new List<TieredPolicyAckOverride>();
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
public sealed class CachedTieredPolicyAckOverrideStore(MemoryTieredPolicyAckOverrideStore inner, TieredPolicyAckOverrideCache cache, int hits, int misses) : ITieredPolicyAckOverrideStore
{
    public MemoryTieredPolicyAckOverrideStore Inner { get; set; } = inner;
    public TieredPolicyAckOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredPolicyAckOverride? Find(string code)
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

    public void Save(TieredPolicyAckOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredPolicyAckOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered policy ack overrides. Empty fields match all.</summary>
public sealed record TieredPolicyAckOverrideQuery(string Source, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(TieredPolicyAckOverride item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredPolicyAckOverride> Apply(List<TieredPolicyAckOverride> items)
    {
        var matched = Collections.Filter(items, (TieredPolicyAckOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered policy ack override store.</summary>
public static class TieredPolicyAckOverrideStore
{
    public static ITieredPolicyAckOverrideStore NewMemoryTieredPolicyAckOverrideStore()
    {
        return new MemoryTieredPolicyAckOverrideStore(
            new Dictionary<string, TieredPolicyAckOverride>(),
            new List<string>());
    }

    public static ITieredPolicyAckOverrideStore NewCachedTieredPolicyAckOverrideStore(MemoryTieredPolicyAckOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedTieredPolicyAckOverrideStore(
            inner,
            TieredPolicyAckOverrideCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<TieredPolicyAckOverride> FindTieredPolicyAckOverrides(ITieredPolicyAckOverrideStore store, TieredPolicyAckOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
