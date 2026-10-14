namespace Acme.Workforce.Talent.Mobility.MobilityRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped mobility notices by code.</summary>
public interface ICappedMobilityNoticeStore
{
    CappedMobilityNotice? Find(string code);
    void Save(CappedMobilityNotice item);
    bool Remove(string code);
    List<CappedMobilityNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedMobilityNoticeStore(Dictionary<string, CappedMobilityNotice> items, List<string> order) : ICappedMobilityNoticeStore
{
    public Dictionary<string, CappedMobilityNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedMobilityNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedMobilityNotice item)
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

    public List<CappedMobilityNotice> List()
    {
        var outValue = new List<CappedMobilityNotice>();
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
public sealed class CachedCappedMobilityNoticeStore(MemoryCappedMobilityNoticeStore inner, CappedMobilityNoticeCache cache, int hits, int misses) : ICappedMobilityNoticeStore
{
    public MemoryCappedMobilityNoticeStore Inner { get; set; } = inner;
    public CappedMobilityNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedMobilityNotice? Find(string code)
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

    public void Save(CappedMobilityNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedMobilityNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped mobility notices. Empty fields match all.</summary>
public sealed record CappedMobilityNoticeQuery(string Category, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(CappedMobilityNotice item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedMobilityNotice> Apply(List<CappedMobilityNotice> items)
    {
        var matched = Collections.Filter(items, (CappedMobilityNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped mobility notice store.</summary>
public static class CappedMobilityNoticeStore
{
    public static ICappedMobilityNoticeStore NewMemoryCappedMobilityNoticeStore()
    {
        return new MemoryCappedMobilityNoticeStore(
            new Dictionary<string, CappedMobilityNotice>(),
            new List<string>());
    }

    public static ICappedMobilityNoticeStore NewCachedCappedMobilityNoticeStore(MemoryCappedMobilityNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedCappedMobilityNoticeStore(inner, CappedMobilityNoticeCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<CappedMobilityNotice> FindCappedMobilityNotices(ICappedMobilityNoticeStore store, CappedMobilityNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
