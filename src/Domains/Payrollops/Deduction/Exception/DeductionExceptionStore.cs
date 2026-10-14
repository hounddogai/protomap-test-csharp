namespace Acme.Workforce.Payrollops.Deduction.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction exceptions by code.</summary>
public interface IDeductionExceptionStore
{
    DeductionException? Find(string code);
    void Save(DeductionException item);
    bool Remove(string code);
    List<DeductionException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionExceptionStore(Dictionary<string, DeductionException> items, List<string> order) : IDeductionExceptionStore
{
    public Dictionary<string, DeductionException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionException item)
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

    public List<DeductionException> List()
    {
        var outValue = new List<DeductionException>();
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
public sealed class CachedDeductionExceptionStore(MemoryDeductionExceptionStore inner, DeductionExceptionCache cache, int hits, int misses) : IDeductionExceptionStore
{
    public MemoryDeductionExceptionStore Inner { get; set; } = inner;
    public DeductionExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionException? Find(string code)
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

    public void Save(DeductionException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction exceptions. Empty fields match all.</summary>
public sealed record DeductionExceptionQuery(string Segment, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionException item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeductionException> Apply(List<DeductionException> items)
    {
        var matched = Collections.Filter(items, (DeductionException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction exception store.</summary>
public static class DeductionExceptionStore
{
    public static IDeductionExceptionStore NewMemoryDeductionExceptionStore()
    {
        return new MemoryDeductionExceptionStore(new Dictionary<string, DeductionException>(), new List<string>());
    }

    public static IDeductionExceptionStore NewCachedDeductionExceptionStore(MemoryDeductionExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionExceptionStore(inner, DeductionExceptionCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DeductionException> FindDeductionExceptions(IDeductionExceptionStore store, DeductionExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
