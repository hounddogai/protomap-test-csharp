namespace Acme.Workforce.Compensation.PayBand.BandTier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled pay band tiers by code.</summary>
public interface IScheduledPayBandTierStore
{
    ScheduledPayBandTier? Find(string code);
    void Save(ScheduledPayBandTier item);
    bool Remove(string code);
    List<ScheduledPayBandTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledPayBandTierStore(Dictionary<string, ScheduledPayBandTier> items, List<string> order) : IScheduledPayBandTierStore
{
    public Dictionary<string, ScheduledPayBandTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledPayBandTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledPayBandTier item)
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

    public List<ScheduledPayBandTier> List()
    {
        var outValue = new List<ScheduledPayBandTier>();
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
public sealed class CachedScheduledPayBandTierStore(MemoryScheduledPayBandTierStore inner, ScheduledPayBandTierCache cache, int hits, int misses) : IScheduledPayBandTierStore
{
    public MemoryScheduledPayBandTierStore Inner { get; set; } = inner;
    public ScheduledPayBandTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledPayBandTier? Find(string code)
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

    public void Save(ScheduledPayBandTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledPayBandTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled pay band tiers. Empty fields match all.</summary>
public sealed record ScheduledPayBandTierQuery(string Label, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledPayBandTier item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledPayBandTier> Apply(List<ScheduledPayBandTier> items)
    {
        var matched = Collections.Filter(items, (ScheduledPayBandTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled pay band tier store.</summary>
public static class ScheduledPayBandTierStore
{
    public static IScheduledPayBandTierStore NewMemoryScheduledPayBandTierStore()
    {
        return new MemoryScheduledPayBandTierStore(
            new Dictionary<string, ScheduledPayBandTier>(),
            new List<string>());
    }

    public static IScheduledPayBandTierStore NewCachedScheduledPayBandTierStore(MemoryScheduledPayBandTierStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledPayBandTierStore(inner, ScheduledPayBandTierCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ScheduledPayBandTier> FindScheduledPayBandTiers(IScheduledPayBandTierStore store, ScheduledPayBandTierQuery query)
    {
        return query.Apply(store.List());
    }
}
