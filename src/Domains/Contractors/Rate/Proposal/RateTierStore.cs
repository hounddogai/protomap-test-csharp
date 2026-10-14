namespace Acme.Workforce.Contractors.Rate.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate tiers by code.</summary>
public interface IRateTierStore
{
    RateTier? Find(string code);
    void Save(RateTier item);
    bool Remove(string code);
    List<RateTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateTierStore(Dictionary<string, RateTier> items, List<string> order) : IRateTierStore
{
    public Dictionary<string, RateTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateTier item)
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

    public List<RateTier> List()
    {
        var outValue = new List<RateTier>();
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
public sealed class CachedRateTierStore(MemoryRateTierStore inner, RateTierCache cache, int hits, int misses) : IRateTierStore
{
    public MemoryRateTierStore Inner { get; set; } = inner;
    public RateTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateTier? Find(string code)
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

    public void Save(RateTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate tiers. Empty fields match all.</summary>
public sealed record RateTierQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(RateTier item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RateTier> Apply(List<RateTier> items)
    {
        var matched = Collections.Filter(items, (RateTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate tier store.</summary>
public static class RateTierStore
{
    public static IRateTierStore NewMemoryRateTierStore()
    {
        return new MemoryRateTierStore(new Dictionary<string, RateTier>(), new List<string>());
    }

    public static IRateTierStore NewCachedRateTierStore(MemoryRateTierStore inner, int ttl, IClock clock)
    {
        return new CachedRateTierStore(inner, RateTierCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RateTier> FindRateTiers(IRateTierStore store, RateTierQuery query)
    {
        return query.Apply(store.List());
    }
}
