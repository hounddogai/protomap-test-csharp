namespace Acme.Workforce.Payrollops.Deduction.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending deduction exceptions by code.</summary>
public interface IPendingDeductionExceptionStore
{
    PendingDeductionException? Find(string code);
    void Save(PendingDeductionException item);
    bool Remove(string code);
    List<PendingDeductionException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingDeductionExceptionStore(Dictionary<string, PendingDeductionException> items, List<string> order) : IPendingDeductionExceptionStore
{
    public Dictionary<string, PendingDeductionException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingDeductionException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingDeductionException item)
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

    public List<PendingDeductionException> List()
    {
        var outValue = new List<PendingDeductionException>();
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
public sealed class CachedPendingDeductionExceptionStore(MemoryPendingDeductionExceptionStore inner, PendingDeductionExceptionCache cache, int hits, int misses) : IPendingDeductionExceptionStore
{
    public MemoryPendingDeductionExceptionStore Inner { get; set; } = inner;
    public PendingDeductionExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingDeductionException? Find(string code)
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

    public void Save(PendingDeductionException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingDeductionException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending deduction exceptions. Empty fields match all.</summary>
public sealed record PendingDeductionExceptionQuery(string Country, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(PendingDeductionException item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingDeductionException> Apply(List<PendingDeductionException> items)
    {
        var matched = Collections.Filter(items, (PendingDeductionException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending deduction exception store.</summary>
public static class PendingDeductionExceptionStore
{
    public static IPendingDeductionExceptionStore NewMemoryPendingDeductionExceptionStore()
    {
        return new MemoryPendingDeductionExceptionStore(
            new Dictionary<string, PendingDeductionException>(),
            new List<string>());
    }

    public static IPendingDeductionExceptionStore NewCachedPendingDeductionExceptionStore(MemoryPendingDeductionExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedPendingDeductionExceptionStore(
            inner,
            PendingDeductionExceptionCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PendingDeductionException> FindPendingDeductionExceptions(IPendingDeductionExceptionStore store, PendingDeductionExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
