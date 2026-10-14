namespace Acme.Workforce.Payrollops.TaxForm.FormBundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient tax form assignments by code.</summary>
public interface ILenientTaxFormAssignmentStore
{
    LenientTaxFormAssignment? Find(string code);
    void Save(LenientTaxFormAssignment item);
    bool Remove(string code);
    List<LenientTaxFormAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientTaxFormAssignmentStore(Dictionary<string, LenientTaxFormAssignment> items, List<string> order) : ILenientTaxFormAssignmentStore
{
    public Dictionary<string, LenientTaxFormAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientTaxFormAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientTaxFormAssignment item)
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

    public List<LenientTaxFormAssignment> List()
    {
        var outValue = new List<LenientTaxFormAssignment>();
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
public sealed class CachedLenientTaxFormAssignmentStore(MemoryLenientTaxFormAssignmentStore inner, LenientTaxFormAssignmentCache cache, int hits, int misses) : ILenientTaxFormAssignmentStore
{
    public MemoryLenientTaxFormAssignmentStore Inner { get; set; } = inner;
    public LenientTaxFormAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientTaxFormAssignment? Find(string code)
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

    public void Save(LenientTaxFormAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientTaxFormAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient tax form assignments. Empty fields match all.</summary>
public sealed record LenientTaxFormAssignmentQuery(string Region, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(LenientTaxFormAssignment item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<LenientTaxFormAssignment> Apply(List<LenientTaxFormAssignment> items)
    {
        var matched = Collections.Filter(items, (LenientTaxFormAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient tax form assignment store.</summary>
public static class LenientTaxFormAssignmentStore
{
    public static ILenientTaxFormAssignmentStore NewMemoryLenientTaxFormAssignmentStore()
    {
        return new MemoryLenientTaxFormAssignmentStore(
            new Dictionary<string, LenientTaxFormAssignment>(),
            new List<string>());
    }

    public static ILenientTaxFormAssignmentStore NewCachedLenientTaxFormAssignmentStore(MemoryLenientTaxFormAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedLenientTaxFormAssignmentStore(
            inner,
            LenientTaxFormAssignmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LenientTaxFormAssignment> FindLenientTaxFormAssignments(ILenientTaxFormAssignmentStore store, LenientTaxFormAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
