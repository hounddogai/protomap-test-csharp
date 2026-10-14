namespace Acme.Workforce.Payrollops.TaxForm.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form notices by code.</summary>
public interface ITaxFormNoticeStore
{
    TaxFormNotice? Find(string code);
    void Save(TaxFormNotice item);
    bool Remove(string code);
    List<TaxFormNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormNoticeStore(Dictionary<string, TaxFormNotice> items, List<string> order) : ITaxFormNoticeStore
{
    public Dictionary<string, TaxFormNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormNotice item)
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

    public List<TaxFormNotice> List()
    {
        var outValue = new List<TaxFormNotice>();
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
public sealed class CachedTaxFormNoticeStore(MemoryTaxFormNoticeStore inner, TaxFormNoticeCache cache, int hits, int misses) : ITaxFormNoticeStore
{
    public MemoryTaxFormNoticeStore Inner { get; set; } = inner;
    public TaxFormNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormNotice? Find(string code)
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

    public void Save(TaxFormNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form notices. Empty fields match all.</summary>
public sealed record TaxFormNoticeQuery(string Label, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormNotice item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TaxFormNotice> Apply(List<TaxFormNotice> items)
    {
        var matched = Collections.Filter(items, (TaxFormNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form notice store.</summary>
public static class TaxFormNoticeStore
{
    public static ITaxFormNoticeStore NewMemoryTaxFormNoticeStore()
    {
        return new MemoryTaxFormNoticeStore(new Dictionary<string, TaxFormNotice>(), new List<string>());
    }

    public static ITaxFormNoticeStore NewCachedTaxFormNoticeStore(MemoryTaxFormNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormNoticeStore(inner, TaxFormNoticeCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TaxFormNotice> FindTaxFormNotices(ITaxFormNoticeStore store, TaxFormNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
