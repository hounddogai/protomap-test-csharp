namespace Acme.Workforce.Payrollops.Deduction.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual deduction notices by code.</summary>
public interface IManualDeductionNoticeStore
{
    ManualDeductionNotice? Find(string code);
    void Save(ManualDeductionNotice item);
    bool Remove(string code);
    List<ManualDeductionNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualDeductionNoticeStore(Dictionary<string, ManualDeductionNotice> items, List<string> order) : IManualDeductionNoticeStore
{
    public Dictionary<string, ManualDeductionNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualDeductionNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualDeductionNotice item)
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

    public List<ManualDeductionNotice> List()
    {
        var outValue = new List<ManualDeductionNotice>();
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
public sealed class CachedManualDeductionNoticeStore(MemoryManualDeductionNoticeStore inner, ManualDeductionNoticeCache cache, int hits, int misses) : IManualDeductionNoticeStore
{
    public MemoryManualDeductionNoticeStore Inner { get; set; } = inner;
    public ManualDeductionNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualDeductionNotice? Find(string code)
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

    public void Save(ManualDeductionNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualDeductionNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual deduction notices. Empty fields match all.</summary>
public sealed record ManualDeductionNoticeQuery(string Unit, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(ManualDeductionNotice item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualDeductionNotice> Apply(List<ManualDeductionNotice> items)
    {
        var matched = Collections.Filter(items, (ManualDeductionNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual deduction notice store.</summary>
public static class ManualDeductionNoticeStore
{
    public static IManualDeductionNoticeStore NewMemoryManualDeductionNoticeStore()
    {
        return new MemoryManualDeductionNoticeStore(
            new Dictionary<string, ManualDeductionNotice>(),
            new List<string>());
    }

    public static IManualDeductionNoticeStore NewCachedManualDeductionNoticeStore(MemoryManualDeductionNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedManualDeductionNoticeStore(inner, ManualDeductionNoticeCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ManualDeductionNotice> FindManualDeductionNotices(IManualDeductionNoticeStore store, ManualDeductionNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
