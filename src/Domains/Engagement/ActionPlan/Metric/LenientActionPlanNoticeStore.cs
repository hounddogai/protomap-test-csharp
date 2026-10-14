namespace Acme.Workforce.Engagement.ActionPlan.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient action plan notices by code.</summary>
public interface ILenientActionPlanNoticeStore
{
    LenientActionPlanNotice? Find(string code);
    void Save(LenientActionPlanNotice item);
    bool Remove(string code);
    List<LenientActionPlanNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientActionPlanNoticeStore(Dictionary<string, LenientActionPlanNotice> items, List<string> order) : ILenientActionPlanNoticeStore
{
    public Dictionary<string, LenientActionPlanNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientActionPlanNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientActionPlanNotice item)
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

    public List<LenientActionPlanNotice> List()
    {
        var outValue = new List<LenientActionPlanNotice>();
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
public sealed class CachedLenientActionPlanNoticeStore(MemoryLenientActionPlanNoticeStore inner, LenientActionPlanNoticeCache cache, int hits, int misses) : ILenientActionPlanNoticeStore
{
    public MemoryLenientActionPlanNoticeStore Inner { get; set; } = inner;
    public LenientActionPlanNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientActionPlanNotice? Find(string code)
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

    public void Save(LenientActionPlanNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientActionPlanNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient action plan notices. Empty fields match all.</summary>
public sealed record LenientActionPlanNoticeQuery(string Segment, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(LenientActionPlanNotice item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<LenientActionPlanNotice> Apply(List<LenientActionPlanNotice> items)
    {
        var matched = Collections.Filter(items, (LenientActionPlanNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient action plan notice store.</summary>
public static class LenientActionPlanNoticeStore
{
    public static ILenientActionPlanNoticeStore NewMemoryLenientActionPlanNoticeStore()
    {
        return new MemoryLenientActionPlanNoticeStore(
            new Dictionary<string, LenientActionPlanNotice>(),
            new List<string>());
    }

    public static ILenientActionPlanNoticeStore NewCachedLenientActionPlanNoticeStore(MemoryLenientActionPlanNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedLenientActionPlanNoticeStore(
            inner,
            LenientActionPlanNoticeCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LenientActionPlanNotice> FindLenientActionPlanNotices(ILenientActionPlanNoticeStore store, LenientActionPlanNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
