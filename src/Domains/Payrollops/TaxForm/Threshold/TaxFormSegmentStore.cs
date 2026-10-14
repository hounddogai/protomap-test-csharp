namespace Acme.Workforce.Payrollops.TaxForm.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form segments by code.</summary>
public interface ITaxFormSegmentStore
{
    TaxFormSegment? Find(string code);
    void Save(TaxFormSegment item);
    bool Remove(string code);
    List<TaxFormSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormSegmentStore(Dictionary<string, TaxFormSegment> items, List<string> order) : ITaxFormSegmentStore
{
    public Dictionary<string, TaxFormSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormSegment item)
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

    public List<TaxFormSegment> List()
    {
        var outValue = new List<TaxFormSegment>();
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
public sealed class CachedTaxFormSegmentStore(MemoryTaxFormSegmentStore inner, TaxFormSegmentCache cache, int hits, int misses) : ITaxFormSegmentStore
{
    public MemoryTaxFormSegmentStore Inner { get; set; } = inner;
    public TaxFormSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormSegment? Find(string code)
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

    public void Save(TaxFormSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form segments. Empty fields match all.</summary>
public sealed record TaxFormSegmentQuery(string Team, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormSegment item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<TaxFormSegment> Apply(List<TaxFormSegment> items)
    {
        var matched = Collections.Filter(items, (TaxFormSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form segment store.</summary>
public static class TaxFormSegmentStore
{
    public static ITaxFormSegmentStore NewMemoryTaxFormSegmentStore()
    {
        return new MemoryTaxFormSegmentStore(new Dictionary<string, TaxFormSegment>(), new List<string>());
    }

    public static ITaxFormSegmentStore NewCachedTaxFormSegmentStore(MemoryTaxFormSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormSegmentStore(inner, TaxFormSegmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TaxFormSegment> FindTaxFormSegments(ITaxFormSegmentStore store, TaxFormSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
