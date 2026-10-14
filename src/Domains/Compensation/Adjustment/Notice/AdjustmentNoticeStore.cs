namespace Acme.Workforce.Compensation.Adjustment.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment notices by code.</summary>
public interface IAdjustmentNoticeStore
{
    AdjustmentNotice? Find(string code);
    void Save(AdjustmentNotice item);
    bool Remove(string code);
    List<AdjustmentNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentNoticeStore(Dictionary<string, AdjustmentNotice> items, List<string> order) : IAdjustmentNoticeStore
{
    public Dictionary<string, AdjustmentNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentNotice item)
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

    public List<AdjustmentNotice> List()
    {
        var outValue = new List<AdjustmentNotice>();
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
public sealed class CachedAdjustmentNoticeStore(MemoryAdjustmentNoticeStore inner, AdjustmentNoticeCache cache, int hits, int misses) : IAdjustmentNoticeStore
{
    public MemoryAdjustmentNoticeStore Inner { get; set; } = inner;
    public AdjustmentNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentNotice? Find(string code)
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

    public void Save(AdjustmentNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment notices. Empty fields match all.</summary>
public sealed record AdjustmentNoticeQuery(string Label, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentNotice item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<AdjustmentNotice> Apply(List<AdjustmentNotice> items)
    {
        var matched = Collections.Filter(items, (AdjustmentNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment notice store.</summary>
public static class AdjustmentNoticeStore
{
    public static IAdjustmentNoticeStore NewMemoryAdjustmentNoticeStore()
    {
        return new MemoryAdjustmentNoticeStore(new Dictionary<string, AdjustmentNotice>(), new List<string>());
    }

    public static IAdjustmentNoticeStore NewCachedAdjustmentNoticeStore(MemoryAdjustmentNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentNoticeStore(inner, AdjustmentNoticeCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AdjustmentNotice> FindAdjustmentNotices(IAdjustmentNoticeStore store, AdjustmentNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
