namespace Acme.Workforce.Orgdesign.Reorg.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict reorg notices by code.</summary>
public interface IStrictReorgNoticeStore
{
    StrictReorgNotice? Find(string code);
    void Save(StrictReorgNotice item);
    bool Remove(string code);
    List<StrictReorgNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictReorgNoticeStore(Dictionary<string, StrictReorgNotice> items, List<string> order) : IStrictReorgNoticeStore
{
    public Dictionary<string, StrictReorgNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictReorgNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictReorgNotice item)
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

    public List<StrictReorgNotice> List()
    {
        var outValue = new List<StrictReorgNotice>();
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
public sealed class CachedStrictReorgNoticeStore(MemoryStrictReorgNoticeStore inner, StrictReorgNoticeCache cache, int hits, int misses) : IStrictReorgNoticeStore
{
    public MemoryStrictReorgNoticeStore Inner { get; set; } = inner;
    public StrictReorgNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictReorgNotice? Find(string code)
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

    public void Save(StrictReorgNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictReorgNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict reorg notices. Empty fields match all.</summary>
public sealed record StrictReorgNoticeQuery(string Source, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(StrictReorgNotice item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<StrictReorgNotice> Apply(List<StrictReorgNotice> items)
    {
        var matched = Collections.Filter(items, (StrictReorgNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict reorg notice store.</summary>
public static class StrictReorgNoticeStore
{
    public static IStrictReorgNoticeStore NewMemoryStrictReorgNoticeStore()
    {
        return new MemoryStrictReorgNoticeStore(new Dictionary<string, StrictReorgNotice>(), new List<string>());
    }

    public static IStrictReorgNoticeStore NewCachedStrictReorgNoticeStore(MemoryStrictReorgNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedStrictReorgNoticeStore(inner, StrictReorgNoticeCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<StrictReorgNotice> FindStrictReorgNotices(IStrictReorgNoticeStore store, StrictReorgNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
