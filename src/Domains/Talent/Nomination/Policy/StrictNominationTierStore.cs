namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict nomination tiers by code.</summary>
public interface IStrictNominationTierStore
{
    StrictNominationTier? Find(string code);
    void Save(StrictNominationTier item);
    bool Remove(string code);
    List<StrictNominationTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictNominationTierStore(Dictionary<string, StrictNominationTier> items, List<string> order) : IStrictNominationTierStore
{
    public Dictionary<string, StrictNominationTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictNominationTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictNominationTier item)
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

    public List<StrictNominationTier> List()
    {
        var outValue = new List<StrictNominationTier>();
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
public sealed class CachedStrictNominationTierStore(MemoryStrictNominationTierStore inner, StrictNominationTierCache cache, int hits, int misses) : IStrictNominationTierStore
{
    public MemoryStrictNominationTierStore Inner { get; set; } = inner;
    public StrictNominationTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictNominationTier? Find(string code)
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

    public void Save(StrictNominationTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictNominationTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict nomination tiers. Empty fields match all.</summary>
public sealed record StrictNominationTierQuery(string Title, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(StrictNominationTier item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictNominationTier> Apply(List<StrictNominationTier> items)
    {
        var matched = Collections.Filter(items, (StrictNominationTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict nomination tier store.</summary>
public static class StrictNominationTierStore
{
    public static IStrictNominationTierStore NewMemoryStrictNominationTierStore()
    {
        return new MemoryStrictNominationTierStore(
            new Dictionary<string, StrictNominationTier>(),
            new List<string>());
    }

    public static IStrictNominationTierStore NewCachedStrictNominationTierStore(MemoryStrictNominationTierStore inner, int ttl, IClock clock)
    {
        return new CachedStrictNominationTierStore(inner, StrictNominationTierCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<StrictNominationTier> FindStrictNominationTiers(IStrictNominationTierStore store, StrictNominationTierQuery query)
    {
        return query.Apply(store.List());
    }
}
