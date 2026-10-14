namespace Acme.Workforce.Absence.Accrual.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary accrual notices by code.</summary>
public interface IPrimaryAccrualNoticeStore
{
    PrimaryAccrualNotice? Find(string code);
    void Save(PrimaryAccrualNotice item);
    bool Remove(string code);
    List<PrimaryAccrualNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryAccrualNoticeStore(Dictionary<string, PrimaryAccrualNotice> items, List<string> order) : IPrimaryAccrualNoticeStore
{
    public Dictionary<string, PrimaryAccrualNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryAccrualNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryAccrualNotice item)
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

    public List<PrimaryAccrualNotice> List()
    {
        var outValue = new List<PrimaryAccrualNotice>();
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
public sealed class CachedPrimaryAccrualNoticeStore(MemoryPrimaryAccrualNoticeStore inner, PrimaryAccrualNoticeCache cache, int hits, int misses) : IPrimaryAccrualNoticeStore
{
    public MemoryPrimaryAccrualNoticeStore Inner { get; set; } = inner;
    public PrimaryAccrualNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryAccrualNotice? Find(string code)
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

    public void Save(PrimaryAccrualNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryAccrualNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary accrual notices. Empty fields match all.</summary>
public sealed record PrimaryAccrualNoticeQuery(string Country, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryAccrualNotice item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<PrimaryAccrualNotice> Apply(List<PrimaryAccrualNotice> items)
    {
        var matched = Collections.Filter(items, (PrimaryAccrualNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary accrual notice store.</summary>
public static class PrimaryAccrualNoticeStore
{
    public static IPrimaryAccrualNoticeStore NewMemoryPrimaryAccrualNoticeStore()
    {
        return new MemoryPrimaryAccrualNoticeStore(
            new Dictionary<string, PrimaryAccrualNotice>(),
            new List<string>());
    }

    public static IPrimaryAccrualNoticeStore NewCachedPrimaryAccrualNoticeStore(MemoryPrimaryAccrualNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryAccrualNoticeStore(inner, PrimaryAccrualNoticeCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PrimaryAccrualNotice> FindPrimaryAccrualNotices(IPrimaryAccrualNoticeStore store, PrimaryAccrualNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
