namespace Acme.Workforce.Compensation.Adjustment.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted adjustment notices by code.</summary>
public interface IWeightedAdjustmentNoticeStore
{
    WeightedAdjustmentNotice? Find(string code);
    void Save(WeightedAdjustmentNotice item);
    bool Remove(string code);
    List<WeightedAdjustmentNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedAdjustmentNoticeStore(Dictionary<string, WeightedAdjustmentNotice> items, List<string> order) : IWeightedAdjustmentNoticeStore
{
    public Dictionary<string, WeightedAdjustmentNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedAdjustmentNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedAdjustmentNotice item)
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

    public List<WeightedAdjustmentNotice> List()
    {
        var outValue = new List<WeightedAdjustmentNotice>();
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
public sealed class CachedWeightedAdjustmentNoticeStore(MemoryWeightedAdjustmentNoticeStore inner, WeightedAdjustmentNoticeCache cache, int hits, int misses) : IWeightedAdjustmentNoticeStore
{
    public MemoryWeightedAdjustmentNoticeStore Inner { get; set; } = inner;
    public WeightedAdjustmentNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedAdjustmentNotice? Find(string code)
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

    public void Save(WeightedAdjustmentNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedAdjustmentNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted adjustment notices. Empty fields match all.</summary>
public sealed record WeightedAdjustmentNoticeQuery(string Team, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedAdjustmentNotice item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedAdjustmentNotice> Apply(List<WeightedAdjustmentNotice> items)
    {
        var matched = Collections.Filter(items, (WeightedAdjustmentNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted adjustment notice store.</summary>
public static class WeightedAdjustmentNoticeStore
{
    public static IWeightedAdjustmentNoticeStore NewMemoryWeightedAdjustmentNoticeStore()
    {
        return new MemoryWeightedAdjustmentNoticeStore(
            new Dictionary<string, WeightedAdjustmentNotice>(),
            new List<string>());
    }

    public static IWeightedAdjustmentNoticeStore NewCachedWeightedAdjustmentNoticeStore(MemoryWeightedAdjustmentNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedAdjustmentNoticeStore(
            inner,
            WeightedAdjustmentNoticeCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<WeightedAdjustmentNotice> FindWeightedAdjustmentNotices(IWeightedAdjustmentNoticeStore store, WeightedAdjustmentNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
