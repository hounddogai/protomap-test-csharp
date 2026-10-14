namespace Acme.Workforce.Absence.Accrual.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual accrual entries by code.</summary>
public interface IManualAccrualEntryStore
{
    ManualAccrualEntry? Find(string code);
    void Save(ManualAccrualEntry item);
    bool Remove(string code);
    List<ManualAccrualEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualAccrualEntryStore(Dictionary<string, ManualAccrualEntry> items, List<string> order) : IManualAccrualEntryStore
{
    public Dictionary<string, ManualAccrualEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualAccrualEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualAccrualEntry item)
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

    public List<ManualAccrualEntry> List()
    {
        var outValue = new List<ManualAccrualEntry>();
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
public sealed class CachedManualAccrualEntryStore(MemoryManualAccrualEntryStore inner, ManualAccrualEntryCache cache, int hits, int misses) : IManualAccrualEntryStore
{
    public MemoryManualAccrualEntryStore Inner { get; set; } = inner;
    public ManualAccrualEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualAccrualEntry? Find(string code)
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

    public void Save(ManualAccrualEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualAccrualEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual accrual entries. Empty fields match all.</summary>
public sealed record ManualAccrualEntryQuery(string Label, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(ManualAccrualEntry item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualAccrualEntry> Apply(List<ManualAccrualEntry> items)
    {
        var matched = Collections.Filter(items, (ManualAccrualEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual accrual entry store.</summary>
public static class ManualAccrualEntryStore
{
    public static IManualAccrualEntryStore NewMemoryManualAccrualEntryStore()
    {
        return new MemoryManualAccrualEntryStore(new Dictionary<string, ManualAccrualEntry>(), new List<string>());
    }

    public static IManualAccrualEntryStore NewCachedManualAccrualEntryStore(MemoryManualAccrualEntryStore inner, int ttl, IClock clock)
    {
        return new CachedManualAccrualEntryStore(inner, ManualAccrualEntryCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ManualAccrualEntry> FindManualAccrualEntries(IManualAccrualEntryStore store, ManualAccrualEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
