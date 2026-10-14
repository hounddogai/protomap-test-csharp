namespace Acme.Workforce.Contractors.Rate.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered rate requests by code.</summary>
public interface ITieredRateRequestStore
{
    TieredRateRequest? Find(string code);
    void Save(TieredRateRequest item);
    bool Remove(string code);
    List<TieredRateRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredRateRequestStore(Dictionary<string, TieredRateRequest> items, List<string> order) : ITieredRateRequestStore
{
    public Dictionary<string, TieredRateRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredRateRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredRateRequest item)
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

    public List<TieredRateRequest> List()
    {
        var outValue = new List<TieredRateRequest>();
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
public sealed class CachedTieredRateRequestStore(MemoryTieredRateRequestStore inner, TieredRateRequestCache cache, int hits, int misses) : ITieredRateRequestStore
{
    public MemoryTieredRateRequestStore Inner { get; set; } = inner;
    public TieredRateRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredRateRequest? Find(string code)
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

    public void Save(TieredRateRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredRateRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered rate requests. Empty fields match all.</summary>
public sealed record TieredRateRequestQuery(string Team, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(TieredRateRequest item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredRateRequest> Apply(List<TieredRateRequest> items)
    {
        var matched = Collections.Filter(items, (TieredRateRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered rate request store.</summary>
public static class TieredRateRequestStore
{
    public static ITieredRateRequestStore NewMemoryTieredRateRequestStore()
    {
        return new MemoryTieredRateRequestStore(new Dictionary<string, TieredRateRequest>(), new List<string>());
    }

    public static ITieredRateRequestStore NewCachedTieredRateRequestStore(MemoryTieredRateRequestStore inner, int ttl, IClock clock)
    {
        return new CachedTieredRateRequestStore(inner, TieredRateRequestCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TieredRateRequest> FindTieredRateRequests(ITieredRateRequestStore store, TieredRateRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
