namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient tax form notices by code.</summary>
public interface ILenientTaxFormNoticeStore
{
    LenientTaxFormNotice? Find(string code);
    void Save(LenientTaxFormNotice item);
    bool Remove(string code);
    List<LenientTaxFormNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientTaxFormNoticeStore(Dictionary<string, LenientTaxFormNotice> items, List<string> order) : ILenientTaxFormNoticeStore
{
    public Dictionary<string, LenientTaxFormNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientTaxFormNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientTaxFormNotice item)
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

    public List<LenientTaxFormNotice> List()
    {
        var outValue = new List<LenientTaxFormNotice>();
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
public sealed class CachedLenientTaxFormNoticeStore(MemoryLenientTaxFormNoticeStore inner, LenientTaxFormNoticeCache cache, int hits, int misses) : ILenientTaxFormNoticeStore
{
    public MemoryLenientTaxFormNoticeStore Inner { get; set; } = inner;
    public LenientTaxFormNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientTaxFormNotice? Find(string code)
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

    public void Save(LenientTaxFormNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientTaxFormNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient tax form notices. Empty fields match all.</summary>
public sealed record LenientTaxFormNoticeQuery(string Name, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(LenientTaxFormNotice item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientTaxFormNotice> Apply(List<LenientTaxFormNotice> items)
    {
        var matched = Collections.Filter(items, (LenientTaxFormNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient tax form notice store.</summary>
public static class LenientTaxFormNoticeStore
{
    public static ILenientTaxFormNoticeStore NewMemoryLenientTaxFormNoticeStore()
    {
        return new MemoryLenientTaxFormNoticeStore(
            new Dictionary<string, LenientTaxFormNotice>(),
            new List<string>());
    }

    public static ILenientTaxFormNoticeStore NewCachedLenientTaxFormNoticeStore(MemoryLenientTaxFormNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedLenientTaxFormNoticeStore(inner, LenientTaxFormNoticeCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LenientTaxFormNotice> FindLenientTaxFormNotices(ILenientTaxFormNoticeStore store, LenientTaxFormNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
