namespace Acme.Workforce.Compensation.PayBand.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal pay band quotas by code.</summary>
public interface ISeasonalPayBandQuotaStore
{
    SeasonalPayBandQuota? Find(string code);
    void Save(SeasonalPayBandQuota item);
    bool Remove(string code);
    List<SeasonalPayBandQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalPayBandQuotaStore(Dictionary<string, SeasonalPayBandQuota> items, List<string> order) : ISeasonalPayBandQuotaStore
{
    public Dictionary<string, SeasonalPayBandQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalPayBandQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalPayBandQuota item)
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

    public List<SeasonalPayBandQuota> List()
    {
        var outValue = new List<SeasonalPayBandQuota>();
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
public sealed class CachedSeasonalPayBandQuotaStore(MemorySeasonalPayBandQuotaStore inner, SeasonalPayBandQuotaCache cache, int hits, int misses) : ISeasonalPayBandQuotaStore
{
    public MemorySeasonalPayBandQuotaStore Inner { get; set; } = inner;
    public SeasonalPayBandQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalPayBandQuota? Find(string code)
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

    public void Save(SeasonalPayBandQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalPayBandQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal pay band quotas. Empty fields match all.</summary>
public sealed record SeasonalPayBandQuotaQuery(string Region, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalPayBandQuota item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalPayBandQuota> Apply(List<SeasonalPayBandQuota> items)
    {
        var matched = Collections.Filter(items, (SeasonalPayBandQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal pay band quota store.</summary>
public static class SeasonalPayBandQuotaStore
{
    public static ISeasonalPayBandQuotaStore NewMemorySeasonalPayBandQuotaStore()
    {
        return new MemorySeasonalPayBandQuotaStore(
            new Dictionary<string, SeasonalPayBandQuota>(),
            new List<string>());
    }

    public static ISeasonalPayBandQuotaStore NewCachedSeasonalPayBandQuotaStore(MemorySeasonalPayBandQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalPayBandQuotaStore(inner, SeasonalPayBandQuotaCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SeasonalPayBandQuota> FindSeasonalPayBandQuotas(ISeasonalPayBandQuotaStore store, SeasonalPayBandQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
