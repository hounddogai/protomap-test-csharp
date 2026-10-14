namespace Acme.Workforce.Contractors.Rate.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate notices by code.</summary>
public interface IRateNoticeStore
{
    RateNotice? Find(string code);
    void Save(RateNotice item);
    bool Remove(string code);
    List<RateNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateNoticeStore(Dictionary<string, RateNotice> items, List<string> order) : IRateNoticeStore
{
    public Dictionary<string, RateNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateNotice item)
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

    public List<RateNotice> List()
    {
        var outValue = new List<RateNotice>();
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
public sealed class CachedRateNoticeStore(MemoryRateNoticeStore inner, RateNoticeCache cache, int hits, int misses) : IRateNoticeStore
{
    public MemoryRateNoticeStore Inner { get; set; } = inner;
    public RateNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateNotice? Find(string code)
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

    public void Save(RateNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate notices. Empty fields match all.</summary>
public sealed record RateNoticeQuery(string Label, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(RateNotice item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RateNotice> Apply(List<RateNotice> items)
    {
        var matched = Collections.Filter(items, (RateNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate notice store.</summary>
public static class RateNoticeStore
{
    public static IRateNoticeStore NewMemoryRateNoticeStore()
    {
        return new MemoryRateNoticeStore(new Dictionary<string, RateNotice>(), new List<string>());
    }

    public static IRateNoticeStore NewCachedRateNoticeStore(MemoryRateNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedRateNoticeStore(inner, RateNoticeCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RateNotice> FindRateNotices(IRateNoticeStore store, RateNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
