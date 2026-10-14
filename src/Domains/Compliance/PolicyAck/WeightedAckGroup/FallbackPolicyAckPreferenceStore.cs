namespace Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback policy ack preferences by code.</summary>
public interface IFallbackPolicyAckPreferenceStore
{
    FallbackPolicyAckPreference? Find(string code);
    void Save(FallbackPolicyAckPreference item);
    bool Remove(string code);
    List<FallbackPolicyAckPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackPolicyAckPreferenceStore(Dictionary<string, FallbackPolicyAckPreference> items, List<string> order) : IFallbackPolicyAckPreferenceStore
{
    public Dictionary<string, FallbackPolicyAckPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackPolicyAckPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackPolicyAckPreference item)
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

    public List<FallbackPolicyAckPreference> List()
    {
        var outValue = new List<FallbackPolicyAckPreference>();
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
public sealed class CachedFallbackPolicyAckPreferenceStore(MemoryFallbackPolicyAckPreferenceStore inner, FallbackPolicyAckPreferenceCache cache, int hits, int misses) : IFallbackPolicyAckPreferenceStore
{
    public MemoryFallbackPolicyAckPreferenceStore Inner { get; set; } = inner;
    public FallbackPolicyAckPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackPolicyAckPreference? Find(string code)
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

    public void Save(FallbackPolicyAckPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackPolicyAckPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback policy ack preferences. Empty fields match all.</summary>
public sealed record FallbackPolicyAckPreferenceQuery(string Unit, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackPolicyAckPreference item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackPolicyAckPreference> Apply(List<FallbackPolicyAckPreference> items)
    {
        var matched = Collections.Filter(items, (FallbackPolicyAckPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback policy ack preference store.</summary>
public static class FallbackPolicyAckPreferenceStore
{
    public static IFallbackPolicyAckPreferenceStore NewMemoryFallbackPolicyAckPreferenceStore()
    {
        return new MemoryFallbackPolicyAckPreferenceStore(
            new Dictionary<string, FallbackPolicyAckPreference>(),
            new List<string>());
    }

    public static IFallbackPolicyAckPreferenceStore NewCachedFallbackPolicyAckPreferenceStore(MemoryFallbackPolicyAckPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackPolicyAckPreferenceStore(
            inner,
            FallbackPolicyAckPreferenceCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<FallbackPolicyAckPreference> FindFallbackPolicyAckPreferences(IFallbackPolicyAckPreferenceStore store, FallbackPolicyAckPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
