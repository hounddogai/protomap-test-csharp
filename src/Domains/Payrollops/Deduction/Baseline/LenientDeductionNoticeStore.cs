namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient deduction notices by code.</summary>
public interface ILenientDeductionNoticeStore
{
    LenientDeductionNotice? Find(string code);
    void Save(LenientDeductionNotice item);
    bool Remove(string code);
    List<LenientDeductionNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientDeductionNoticeStore(Dictionary<string, LenientDeductionNotice> items, List<string> order) : ILenientDeductionNoticeStore
{
    public Dictionary<string, LenientDeductionNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientDeductionNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientDeductionNotice item)
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

    public List<LenientDeductionNotice> List()
    {
        var outValue = new List<LenientDeductionNotice>();
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
public sealed class CachedLenientDeductionNoticeStore(MemoryLenientDeductionNoticeStore inner, LenientDeductionNoticeCache cache, int hits, int misses) : ILenientDeductionNoticeStore
{
    public MemoryLenientDeductionNoticeStore Inner { get; set; } = inner;
    public LenientDeductionNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientDeductionNotice? Find(string code)
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

    public void Save(LenientDeductionNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientDeductionNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient deduction notices. Empty fields match all.</summary>
public sealed record LenientDeductionNoticeQuery(string Label, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(LenientDeductionNotice item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<LenientDeductionNotice> Apply(List<LenientDeductionNotice> items)
    {
        var matched = Collections.Filter(items, (LenientDeductionNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient deduction notice store.</summary>
public static class LenientDeductionNoticeStore
{
    public static ILenientDeductionNoticeStore NewMemoryLenientDeductionNoticeStore()
    {
        return new MemoryLenientDeductionNoticeStore(
            new Dictionary<string, LenientDeductionNotice>(),
            new List<string>());
    }

    public static ILenientDeductionNoticeStore NewCachedLenientDeductionNoticeStore(MemoryLenientDeductionNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedLenientDeductionNoticeStore(
            inner,
            LenientDeductionNoticeCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LenientDeductionNotice> FindLenientDeductionNotices(ILenientDeductionNoticeStore store, LenientDeductionNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
