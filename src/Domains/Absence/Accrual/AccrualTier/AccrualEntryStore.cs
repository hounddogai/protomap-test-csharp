namespace Acme.Workforce.Absence.Accrual.AccrualTier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual entries by code.</summary>
public interface IAccrualEntryStore
{
    AccrualEntry? Find(string code);
    void Save(AccrualEntry item);
    bool Remove(string code);
    List<AccrualEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualEntryStore(Dictionary<string, AccrualEntry> items, List<string> order) : IAccrualEntryStore
{
    public Dictionary<string, AccrualEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualEntry item)
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

    public List<AccrualEntry> List()
    {
        var outValue = new List<AccrualEntry>();
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
public sealed class CachedAccrualEntryStore(MemoryAccrualEntryStore inner, AccrualEntryCache cache, int hits, int misses) : IAccrualEntryStore
{
    public MemoryAccrualEntryStore Inner { get; set; } = inner;
    public AccrualEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualEntry? Find(string code)
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

    public void Save(AccrualEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual entries. Empty fields match all.</summary>
public sealed record AccrualEntryQuery(string Label, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualEntry item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<AccrualEntry> Apply(List<AccrualEntry> items)
    {
        var matched = Collections.Filter(items, (AccrualEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual entry store.</summary>
public static class AccrualEntryStore
{
    public static IAccrualEntryStore NewMemoryAccrualEntryStore()
    {
        return new MemoryAccrualEntryStore(new Dictionary<string, AccrualEntry>(), new List<string>());
    }

    public static IAccrualEntryStore NewCachedAccrualEntryStore(MemoryAccrualEntryStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualEntryStore(inner, AccrualEntryCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AccrualEntry> FindAccrualEntries(IAccrualEntryStore store, AccrualEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
