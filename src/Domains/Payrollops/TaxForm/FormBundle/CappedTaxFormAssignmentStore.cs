namespace Acme.Workforce.Payrollops.TaxForm.FormBundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped tax form assignments by code.</summary>
public interface ICappedTaxFormAssignmentStore
{
    CappedTaxFormAssignment? Find(string code);
    void Save(CappedTaxFormAssignment item);
    bool Remove(string code);
    List<CappedTaxFormAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedTaxFormAssignmentStore(Dictionary<string, CappedTaxFormAssignment> items, List<string> order) : ICappedTaxFormAssignmentStore
{
    public Dictionary<string, CappedTaxFormAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedTaxFormAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedTaxFormAssignment item)
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

    public List<CappedTaxFormAssignment> List()
    {
        var outValue = new List<CappedTaxFormAssignment>();
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
public sealed class CachedCappedTaxFormAssignmentStore(MemoryCappedTaxFormAssignmentStore inner, CappedTaxFormAssignmentCache cache, int hits, int misses) : ICappedTaxFormAssignmentStore
{
    public MemoryCappedTaxFormAssignmentStore Inner { get; set; } = inner;
    public CappedTaxFormAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedTaxFormAssignment? Find(string code)
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

    public void Save(CappedTaxFormAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedTaxFormAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped tax form assignments. Empty fields match all.</summary>
public sealed record CappedTaxFormAssignmentQuery(string Source, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(CappedTaxFormAssignment item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedTaxFormAssignment> Apply(List<CappedTaxFormAssignment> items)
    {
        var matched = Collections.Filter(items, (CappedTaxFormAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped tax form assignment store.</summary>
public static class CappedTaxFormAssignmentStore
{
    public static ICappedTaxFormAssignmentStore NewMemoryCappedTaxFormAssignmentStore()
    {
        return new MemoryCappedTaxFormAssignmentStore(
            new Dictionary<string, CappedTaxFormAssignment>(),
            new List<string>());
    }

    public static ICappedTaxFormAssignmentStore NewCachedCappedTaxFormAssignmentStore(MemoryCappedTaxFormAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedCappedTaxFormAssignmentStore(
            inner,
            CappedTaxFormAssignmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<CappedTaxFormAssignment> FindCappedTaxFormAssignments(ICappedTaxFormAssignmentStore store, CappedTaxFormAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
