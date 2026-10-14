namespace Acme.Workforce.Absence.Accrual.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual exceptions by code.</summary>
public interface IAccrualExceptionStore
{
    AccrualException? Find(string code);
    void Save(AccrualException item);
    bool Remove(string code);
    List<AccrualException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualExceptionStore(Dictionary<string, AccrualException> items, List<string> order) : IAccrualExceptionStore
{
    public Dictionary<string, AccrualException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualException item)
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

    public List<AccrualException> List()
    {
        var outValue = new List<AccrualException>();
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
public sealed class CachedAccrualExceptionStore(MemoryAccrualExceptionStore inner, AccrualExceptionCache cache, int hits, int misses) : IAccrualExceptionStore
{
    public MemoryAccrualExceptionStore Inner { get; set; } = inner;
    public AccrualExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualException? Find(string code)
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

    public void Save(AccrualException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual exceptions. Empty fields match all.</summary>
public sealed record AccrualExceptionQuery(string Name, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualException item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<AccrualException> Apply(List<AccrualException> items)
    {
        var matched = Collections.Filter(items, (AccrualException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual exception store.</summary>
public static class AccrualExceptionStore
{
    public static IAccrualExceptionStore NewMemoryAccrualExceptionStore()
    {
        return new MemoryAccrualExceptionStore(new Dictionary<string, AccrualException>(), new List<string>());
    }

    public static IAccrualExceptionStore NewCachedAccrualExceptionStore(MemoryAccrualExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualExceptionStore(inner, AccrualExceptionCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AccrualException> FindAccrualExceptions(IAccrualExceptionStore store, AccrualExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
