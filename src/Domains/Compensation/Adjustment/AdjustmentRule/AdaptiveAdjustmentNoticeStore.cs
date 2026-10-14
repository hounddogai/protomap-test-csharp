namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive adjustment notices by code.</summary>
public interface IAdaptiveAdjustmentNoticeStore
{
    AdaptiveAdjustmentNotice? Find(string code);
    void Save(AdaptiveAdjustmentNotice item);
    bool Remove(string code);
    List<AdaptiveAdjustmentNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveAdjustmentNoticeStore(Dictionary<string, AdaptiveAdjustmentNotice> items, List<string> order) : IAdaptiveAdjustmentNoticeStore
{
    public Dictionary<string, AdaptiveAdjustmentNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveAdjustmentNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveAdjustmentNotice item)
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

    public List<AdaptiveAdjustmentNotice> List()
    {
        var outValue = new List<AdaptiveAdjustmentNotice>();
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
public sealed class CachedAdaptiveAdjustmentNoticeStore(MemoryAdaptiveAdjustmentNoticeStore inner, AdaptiveAdjustmentNoticeCache cache, int hits, int misses) : IAdaptiveAdjustmentNoticeStore
{
    public MemoryAdaptiveAdjustmentNoticeStore Inner { get; set; } = inner;
    public AdaptiveAdjustmentNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveAdjustmentNotice? Find(string code)
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

    public void Save(AdaptiveAdjustmentNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveAdjustmentNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive adjustment notices. Empty fields match all.</summary>
public sealed record AdaptiveAdjustmentNoticeQuery(string Channel, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveAdjustmentNotice item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveAdjustmentNotice> Apply(List<AdaptiveAdjustmentNotice> items)
    {
        var matched = Collections.Filter(items, (AdaptiveAdjustmentNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive adjustment notice store.</summary>
public static class AdaptiveAdjustmentNoticeStore
{
    public static IAdaptiveAdjustmentNoticeStore NewMemoryAdaptiveAdjustmentNoticeStore()
    {
        return new MemoryAdaptiveAdjustmentNoticeStore(
            new Dictionary<string, AdaptiveAdjustmentNotice>(),
            new List<string>());
    }

    public static IAdaptiveAdjustmentNoticeStore NewCachedAdaptiveAdjustmentNoticeStore(MemoryAdaptiveAdjustmentNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveAdjustmentNoticeStore(
            inner,
            AdaptiveAdjustmentNoticeCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveAdjustmentNotice> FindAdaptiveAdjustmentNotices(IAdaptiveAdjustmentNoticeStore store, AdaptiveAdjustmentNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
