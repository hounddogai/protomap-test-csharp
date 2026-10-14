namespace Acme.Workforce.Compensation.Benchmark.AdaptiveBenchmarkAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive benchmark notices by code.</summary>
public interface IAdaptiveBenchmarkNoticeStore
{
    AdaptiveBenchmarkNotice? Find(string code);
    void Save(AdaptiveBenchmarkNotice item);
    bool Remove(string code);
    List<AdaptiveBenchmarkNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveBenchmarkNoticeStore(Dictionary<string, AdaptiveBenchmarkNotice> items, List<string> order) : IAdaptiveBenchmarkNoticeStore
{
    public Dictionary<string, AdaptiveBenchmarkNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveBenchmarkNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveBenchmarkNotice item)
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

    public List<AdaptiveBenchmarkNotice> List()
    {
        var outValue = new List<AdaptiveBenchmarkNotice>();
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
public sealed class CachedAdaptiveBenchmarkNoticeStore(MemoryAdaptiveBenchmarkNoticeStore inner, AdaptiveBenchmarkNoticeCache cache, int hits, int misses) : IAdaptiveBenchmarkNoticeStore
{
    public MemoryAdaptiveBenchmarkNoticeStore Inner { get; set; } = inner;
    public AdaptiveBenchmarkNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveBenchmarkNotice? Find(string code)
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

    public void Save(AdaptiveBenchmarkNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveBenchmarkNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive benchmark notices. Empty fields match all.</summary>
public sealed record AdaptiveBenchmarkNoticeQuery(string Label, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveBenchmarkNotice item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveBenchmarkNotice> Apply(List<AdaptiveBenchmarkNotice> items)
    {
        var matched = Collections.Filter(items, (AdaptiveBenchmarkNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive benchmark notice store.</summary>
public static class AdaptiveBenchmarkNoticeStore
{
    public static IAdaptiveBenchmarkNoticeStore NewMemoryAdaptiveBenchmarkNoticeStore()
    {
        return new MemoryAdaptiveBenchmarkNoticeStore(
            new Dictionary<string, AdaptiveBenchmarkNotice>(),
            new List<string>());
    }

    public static IAdaptiveBenchmarkNoticeStore NewCachedAdaptiveBenchmarkNoticeStore(MemoryAdaptiveBenchmarkNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveBenchmarkNoticeStore(
            inner,
            AdaptiveBenchmarkNoticeCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveBenchmarkNotice> FindAdaptiveBenchmarkNotices(IAdaptiveBenchmarkNoticeStore store, AdaptiveBenchmarkNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
