namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction notices by code.</summary>
public interface IDeductionNoticeStore
{
    DeductionNotice? Find(string code);
    void Save(DeductionNotice item);
    bool Remove(string code);
    List<DeductionNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionNoticeStore(Dictionary<string, DeductionNotice> items, List<string> order) : IDeductionNoticeStore
{
    public Dictionary<string, DeductionNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionNotice item)
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

    public List<DeductionNotice> List()
    {
        var outValue = new List<DeductionNotice>();
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
public sealed class CachedDeductionNoticeStore(MemoryDeductionNoticeStore inner, DeductionNoticeCache cache, int hits, int misses) : IDeductionNoticeStore
{
    public MemoryDeductionNoticeStore Inner { get; set; } = inner;
    public DeductionNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionNotice? Find(string code)
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

    public void Save(DeductionNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction notices. Empty fields match all.</summary>
public sealed record DeductionNoticeQuery(string Label, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionNotice item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeductionNotice> Apply(List<DeductionNotice> items)
    {
        var matched = Collections.Filter(items, (DeductionNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction notice store.</summary>
public static class DeductionNoticeStore
{
    public static IDeductionNoticeStore NewMemoryDeductionNoticeStore()
    {
        return new MemoryDeductionNoticeStore(new Dictionary<string, DeductionNotice>(), new List<string>());
    }

    public static IDeductionNoticeStore NewCachedDeductionNoticeStore(MemoryDeductionNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionNoticeStore(inner, DeductionNoticeCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DeductionNotice> FindDeductionNotices(IDeductionNoticeStore store, DeductionNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
