namespace Acme.Workforce.Payrollops.TaxForm.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form assignments by code.</summary>
public interface ITaxFormAssignmentStore
{
    TaxFormAssignment? Find(string code);
    void Save(TaxFormAssignment item);
    bool Remove(string code);
    List<TaxFormAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormAssignmentStore(Dictionary<string, TaxFormAssignment> items, List<string> order) : ITaxFormAssignmentStore
{
    public Dictionary<string, TaxFormAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormAssignment item)
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

    public List<TaxFormAssignment> List()
    {
        var outValue = new List<TaxFormAssignment>();
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
public sealed class CachedTaxFormAssignmentStore(MemoryTaxFormAssignmentStore inner, TaxFormAssignmentCache cache, int hits, int misses) : ITaxFormAssignmentStore
{
    public MemoryTaxFormAssignmentStore Inner { get; set; } = inner;
    public TaxFormAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormAssignment? Find(string code)
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

    public void Save(TaxFormAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form assignments. Empty fields match all.</summary>
public sealed record TaxFormAssignmentQuery(string Category, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormAssignment item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TaxFormAssignment> Apply(List<TaxFormAssignment> items)
    {
        var matched = Collections.Filter(items, (TaxFormAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form assignment store.</summary>
public static class TaxFormAssignmentStore
{
    public static ITaxFormAssignmentStore NewMemoryTaxFormAssignmentStore()
    {
        return new MemoryTaxFormAssignmentStore(new Dictionary<string, TaxFormAssignment>(), new List<string>());
    }

    public static ITaxFormAssignmentStore NewCachedTaxFormAssignmentStore(MemoryTaxFormAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormAssignmentStore(inner, TaxFormAssignmentCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TaxFormAssignment> FindTaxFormAssignments(ITaxFormAssignmentStore store, TaxFormAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
