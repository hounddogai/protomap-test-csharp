namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps engagement tiers by code.</summary>
public interface IEngagementTierStore
{
    EngagementTier? Find(string code);
    void Save(EngagementTier item);
    bool Remove(string code);
    List<EngagementTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEngagementTierStore(Dictionary<string, EngagementTier> items, List<string> order) : IEngagementTierStore
{
    public Dictionary<string, EngagementTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EngagementTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EngagementTier item)
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

    public List<EngagementTier> List()
    {
        var outValue = new List<EngagementTier>();
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
public sealed class CachedEngagementTierStore(MemoryEngagementTierStore inner, EngagementTierCache cache, int hits, int misses) : IEngagementTierStore
{
    public MemoryEngagementTierStore Inner { get; set; } = inner;
    public EngagementTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EngagementTier? Find(string code)
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

    public void Save(EngagementTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EngagementTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects engagement tiers. Empty fields match all.</summary>
public sealed record EngagementTierQuery(string Label, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(EngagementTier item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EngagementTier> Apply(List<EngagementTier> items)
    {
        var matched = Collections.Filter(items, (EngagementTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of engagement tier store.</summary>
public static class EngagementTierStore
{
    public static IEngagementTierStore NewMemoryEngagementTierStore()
    {
        return new MemoryEngagementTierStore(new Dictionary<string, EngagementTier>(), new List<string>());
    }

    public static IEngagementTierStore NewCachedEngagementTierStore(MemoryEngagementTierStore inner, int ttl, IClock clock)
    {
        return new CachedEngagementTierStore(inner, EngagementTierCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<EngagementTier> FindEngagementTiers(IEngagementTierStore store, EngagementTierQuery query)
    {
        return query.Apply(store.List());
    }
}
