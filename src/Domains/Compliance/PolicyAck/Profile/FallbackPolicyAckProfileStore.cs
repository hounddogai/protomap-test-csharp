namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback policy ack profiles by code.</summary>
public interface IFallbackPolicyAckProfileStore
{
    FallbackPolicyAckProfile? Find(string code);
    void Save(FallbackPolicyAckProfile item);
    bool Remove(string code);
    List<FallbackPolicyAckProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackPolicyAckProfileStore(Dictionary<string, FallbackPolicyAckProfile> items, List<string> order) : IFallbackPolicyAckProfileStore
{
    public Dictionary<string, FallbackPolicyAckProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackPolicyAckProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackPolicyAckProfile item)
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

    public List<FallbackPolicyAckProfile> List()
    {
        var outValue = new List<FallbackPolicyAckProfile>();
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
public sealed class CachedFallbackPolicyAckProfileStore(MemoryFallbackPolicyAckProfileStore inner, FallbackPolicyAckProfileCache cache, int hits, int misses) : IFallbackPolicyAckProfileStore
{
    public MemoryFallbackPolicyAckProfileStore Inner { get; set; } = inner;
    public FallbackPolicyAckProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackPolicyAckProfile? Find(string code)
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

    public void Save(FallbackPolicyAckProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackPolicyAckProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback policy ack profiles. Empty fields match all.</summary>
public sealed record FallbackPolicyAckProfileQuery(string Locale, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackPolicyAckProfile item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackPolicyAckProfile> Apply(List<FallbackPolicyAckProfile> items)
    {
        var matched = Collections.Filter(items, (FallbackPolicyAckProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback policy ack profile store.</summary>
public static class FallbackPolicyAckProfileStore
{
    public static IFallbackPolicyAckProfileStore NewMemoryFallbackPolicyAckProfileStore()
    {
        return new MemoryFallbackPolicyAckProfileStore(
            new Dictionary<string, FallbackPolicyAckProfile>(),
            new List<string>());
    }

    public static IFallbackPolicyAckProfileStore NewCachedFallbackPolicyAckProfileStore(MemoryFallbackPolicyAckProfileStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackPolicyAckProfileStore(
            inner,
            FallbackPolicyAckProfileCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<FallbackPolicyAckProfile> FindFallbackPolicyAckProfiles(IFallbackPolicyAckProfileStore store, FallbackPolicyAckProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
