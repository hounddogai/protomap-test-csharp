namespace Acme.Workforce.Engagement.TownHall.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive town hall notices by code.</summary>
public interface IAdaptiveTownHallNoticeStore
{
    AdaptiveTownHallNotice? Find(string code);
    void Save(AdaptiveTownHallNotice item);
    bool Remove(string code);
    List<AdaptiveTownHallNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveTownHallNoticeStore(Dictionary<string, AdaptiveTownHallNotice> items, List<string> order) : IAdaptiveTownHallNoticeStore
{
    public Dictionary<string, AdaptiveTownHallNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveTownHallNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveTownHallNotice item)
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

    public List<AdaptiveTownHallNotice> List()
    {
        var outValue = new List<AdaptiveTownHallNotice>();
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
public sealed class CachedAdaptiveTownHallNoticeStore(MemoryAdaptiveTownHallNoticeStore inner, AdaptiveTownHallNoticeCache cache, int hits, int misses) : IAdaptiveTownHallNoticeStore
{
    public MemoryAdaptiveTownHallNoticeStore Inner { get; set; } = inner;
    public AdaptiveTownHallNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveTownHallNotice? Find(string code)
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

    public void Save(AdaptiveTownHallNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveTownHallNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive town hall notices. Empty fields match all.</summary>
public sealed record AdaptiveTownHallNoticeQuery(string Segment, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveTownHallNotice item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveTownHallNotice> Apply(List<AdaptiveTownHallNotice> items)
    {
        var matched = Collections.Filter(items, (AdaptiveTownHallNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive town hall notice store.</summary>
public static class AdaptiveTownHallNoticeStore
{
    public static IAdaptiveTownHallNoticeStore NewMemoryAdaptiveTownHallNoticeStore()
    {
        return new MemoryAdaptiveTownHallNoticeStore(
            new Dictionary<string, AdaptiveTownHallNotice>(),
            new List<string>());
    }

    public static IAdaptiveTownHallNoticeStore NewCachedAdaptiveTownHallNoticeStore(MemoryAdaptiveTownHallNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveTownHallNoticeStore(
            inner,
            AdaptiveTownHallNoticeCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveTownHallNotice> FindAdaptiveTownHallNotices(IAdaptiveTownHallNoticeStore store, AdaptiveTownHallNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
