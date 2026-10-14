namespace Acme.Workforce.Contractors.Rate.RateNotice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict rate notices by code.</summary>
public interface IStrictRateNoticeStore
{
    StrictRateNotice? Find(string code);
    void Save(StrictRateNotice item);
    bool Remove(string code);
    List<StrictRateNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictRateNoticeStore(Dictionary<string, StrictRateNotice> items, List<string> order) : IStrictRateNoticeStore
{
    public Dictionary<string, StrictRateNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictRateNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictRateNotice item)
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

    public List<StrictRateNotice> List()
    {
        var outValue = new List<StrictRateNotice>();
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
public sealed class CachedStrictRateNoticeStore(MemoryStrictRateNoticeStore inner, StrictRateNoticeCache cache, int hits, int misses) : IStrictRateNoticeStore
{
    public MemoryStrictRateNoticeStore Inner { get; set; } = inner;
    public StrictRateNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictRateNotice? Find(string code)
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

    public void Save(StrictRateNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictRateNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict rate notices. Empty fields match all.</summary>
public sealed record StrictRateNoticeQuery(string Name, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(StrictRateNotice item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictRateNotice> Apply(List<StrictRateNotice> items)
    {
        var matched = Collections.Filter(items, (StrictRateNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict rate notice store.</summary>
public static class StrictRateNoticeStore
{
    public static IStrictRateNoticeStore NewMemoryStrictRateNoticeStore()
    {
        return new MemoryStrictRateNoticeStore(new Dictionary<string, StrictRateNotice>(), new List<string>());
    }

    public static IStrictRateNoticeStore NewCachedStrictRateNoticeStore(MemoryStrictRateNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedStrictRateNoticeStore(inner, StrictRateNoticeCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<StrictRateNotice> FindStrictRateNotices(IStrictRateNoticeStore store, StrictRateNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
