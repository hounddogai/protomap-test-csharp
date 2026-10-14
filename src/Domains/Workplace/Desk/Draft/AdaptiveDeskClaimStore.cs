namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive desk claims by code.</summary>
public interface IAdaptiveDeskClaimStore
{
    AdaptiveDeskClaim? Find(string code);
    void Save(AdaptiveDeskClaim item);
    bool Remove(string code);
    List<AdaptiveDeskClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveDeskClaimStore(Dictionary<string, AdaptiveDeskClaim> items, List<string> order) : IAdaptiveDeskClaimStore
{
    public Dictionary<string, AdaptiveDeskClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveDeskClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveDeskClaim item)
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

    public List<AdaptiveDeskClaim> List()
    {
        var outValue = new List<AdaptiveDeskClaim>();
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
public sealed class CachedAdaptiveDeskClaimStore(MemoryAdaptiveDeskClaimStore inner, AdaptiveDeskClaimCache cache, int hits, int misses) : IAdaptiveDeskClaimStore
{
    public MemoryAdaptiveDeskClaimStore Inner { get; set; } = inner;
    public AdaptiveDeskClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveDeskClaim? Find(string code)
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

    public void Save(AdaptiveDeskClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveDeskClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive desk claims. Empty fields match all.</summary>
public sealed record AdaptiveDeskClaimQuery(string Channel, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveDeskClaim item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<AdaptiveDeskClaim> Apply(List<AdaptiveDeskClaim> items)
    {
        var matched = Collections.Filter(items, (AdaptiveDeskClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive desk claim store.</summary>
public static class AdaptiveDeskClaimStore
{
    public static IAdaptiveDeskClaimStore NewMemoryAdaptiveDeskClaimStore()
    {
        return new MemoryAdaptiveDeskClaimStore(new Dictionary<string, AdaptiveDeskClaim>(), new List<string>());
    }

    public static IAdaptiveDeskClaimStore NewCachedAdaptiveDeskClaimStore(MemoryAdaptiveDeskClaimStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveDeskClaimStore(inner, AdaptiveDeskClaimCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AdaptiveDeskClaim> FindAdaptiveDeskClaims(IAdaptiveDeskClaimStore store, AdaptiveDeskClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
