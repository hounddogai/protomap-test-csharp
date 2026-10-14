namespace Acme.Workforce.Compensation.PayBand.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy pay band requests by code.</summary>
public interface ILegacyPayBandRequestStore
{
    LegacyPayBandRequest? Find(string code);
    void Save(LegacyPayBandRequest item);
    bool Remove(string code);
    List<LegacyPayBandRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyPayBandRequestStore(Dictionary<string, LegacyPayBandRequest> items, List<string> order) : ILegacyPayBandRequestStore
{
    public Dictionary<string, LegacyPayBandRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyPayBandRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyPayBandRequest item)
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

    public List<LegacyPayBandRequest> List()
    {
        var outValue = new List<LegacyPayBandRequest>();
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
public sealed class CachedLegacyPayBandRequestStore(MemoryLegacyPayBandRequestStore inner, LegacyPayBandRequestCache cache, int hits, int misses) : ILegacyPayBandRequestStore
{
    public MemoryLegacyPayBandRequestStore Inner { get; set; } = inner;
    public LegacyPayBandRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyPayBandRequest? Find(string code)
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

    public void Save(LegacyPayBandRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyPayBandRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy pay band requests. Empty fields match all.</summary>
public sealed record LegacyPayBandRequestQuery(string Team, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyPayBandRequest item)
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

    public List<LegacyPayBandRequest> Apply(List<LegacyPayBandRequest> items)
    {
        var matched = Collections.Filter(items, (LegacyPayBandRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy pay band request store.</summary>
public static class LegacyPayBandRequestStore
{
    public static ILegacyPayBandRequestStore NewMemoryLegacyPayBandRequestStore()
    {
        return new MemoryLegacyPayBandRequestStore(
            new Dictionary<string, LegacyPayBandRequest>(),
            new List<string>());
    }

    public static ILegacyPayBandRequestStore NewCachedLegacyPayBandRequestStore(MemoryLegacyPayBandRequestStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyPayBandRequestStore(inner, LegacyPayBandRequestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LegacyPayBandRequest> FindLegacyPayBandRequests(ILegacyPayBandRequestStore store, LegacyPayBandRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
