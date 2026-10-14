namespace Acme.Workforce.Contractors.Offboarding.OffboardingBundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy offboarding requests by code.</summary>
public interface ILegacyOffboardingRequestStore
{
    LegacyOffboardingRequest? Find(string code);
    void Save(LegacyOffboardingRequest item);
    bool Remove(string code);
    List<LegacyOffboardingRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyOffboardingRequestStore(Dictionary<string, LegacyOffboardingRequest> items, List<string> order) : ILegacyOffboardingRequestStore
{
    public Dictionary<string, LegacyOffboardingRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyOffboardingRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyOffboardingRequest item)
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

    public List<LegacyOffboardingRequest> List()
    {
        var outValue = new List<LegacyOffboardingRequest>();
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
public sealed class CachedLegacyOffboardingRequestStore(MemoryLegacyOffboardingRequestStore inner, LegacyOffboardingRequestCache cache, int hits, int misses) : ILegacyOffboardingRequestStore
{
    public MemoryLegacyOffboardingRequestStore Inner { get; set; } = inner;
    public LegacyOffboardingRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyOffboardingRequest? Find(string code)
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

    public void Save(LegacyOffboardingRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyOffboardingRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy offboarding requests. Empty fields match all.</summary>
public sealed record LegacyOffboardingRequestQuery(string Label, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyOffboardingRequest item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<LegacyOffboardingRequest> Apply(List<LegacyOffboardingRequest> items)
    {
        var matched = Collections.Filter(items, (LegacyOffboardingRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy offboarding request store.</summary>
public static class LegacyOffboardingRequestStore
{
    public static ILegacyOffboardingRequestStore NewMemoryLegacyOffboardingRequestStore()
    {
        return new MemoryLegacyOffboardingRequestStore(
            new Dictionary<string, LegacyOffboardingRequest>(),
            new List<string>());
    }

    public static ILegacyOffboardingRequestStore NewCachedLegacyOffboardingRequestStore(MemoryLegacyOffboardingRequestStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyOffboardingRequestStore(
            inner,
            LegacyOffboardingRequestCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LegacyOffboardingRequest> FindLegacyOffboardingRequests(ILegacyOffboardingRequestStore store, LegacyOffboardingRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
