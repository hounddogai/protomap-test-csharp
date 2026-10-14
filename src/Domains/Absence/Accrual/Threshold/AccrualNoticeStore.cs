namespace Acme.Workforce.Absence.Accrual.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual notices by code.</summary>
public interface IAccrualNoticeStore
{
    AccrualNotice? Find(string code);
    void Save(AccrualNotice item);
    bool Remove(string code);
    List<AccrualNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualNoticeStore(Dictionary<string, AccrualNotice> items, List<string> order) : IAccrualNoticeStore
{
    public Dictionary<string, AccrualNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualNotice item)
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

    public List<AccrualNotice> List()
    {
        var outValue = new List<AccrualNotice>();
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
public sealed class CachedAccrualNoticeStore(MemoryAccrualNoticeStore inner, AccrualNoticeCache cache, int hits, int misses) : IAccrualNoticeStore
{
    public MemoryAccrualNoticeStore Inner { get; set; } = inner;
    public AccrualNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualNotice? Find(string code)
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

    public void Save(AccrualNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual notices. Empty fields match all.</summary>
public sealed record AccrualNoticeQuery(string Category, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualNotice item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccrualNotice> Apply(List<AccrualNotice> items)
    {
        var matched = Collections.Filter(items, (AccrualNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual notice store.</summary>
public static class AccrualNoticeStore
{
    public static IAccrualNoticeStore NewMemoryAccrualNoticeStore()
    {
        return new MemoryAccrualNoticeStore(new Dictionary<string, AccrualNotice>(), new List<string>());
    }

    public static IAccrualNoticeStore NewCachedAccrualNoticeStore(MemoryAccrualNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualNoticeStore(inner, AccrualNoticeCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AccrualNotice> FindAccrualNotices(IAccrualNoticeStore store, AccrualNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
