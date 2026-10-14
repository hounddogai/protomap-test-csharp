namespace Acme.Workforce.Payrollops.Deduction.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling deduction entries by code.</summary>
public interface IRollingDeductionEntryStore
{
    RollingDeductionEntry? Find(string code);
    void Save(RollingDeductionEntry item);
    bool Remove(string code);
    List<RollingDeductionEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingDeductionEntryStore(Dictionary<string, RollingDeductionEntry> items, List<string> order) : IRollingDeductionEntryStore
{
    public Dictionary<string, RollingDeductionEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingDeductionEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingDeductionEntry item)
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

    public List<RollingDeductionEntry> List()
    {
        var outValue = new List<RollingDeductionEntry>();
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
public sealed class CachedRollingDeductionEntryStore(MemoryRollingDeductionEntryStore inner, RollingDeductionEntryCache cache, int hits, int misses) : IRollingDeductionEntryStore
{
    public MemoryRollingDeductionEntryStore Inner { get; set; } = inner;
    public RollingDeductionEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingDeductionEntry? Find(string code)
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

    public void Save(RollingDeductionEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingDeductionEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling deduction entries. Empty fields match all.</summary>
public sealed record RollingDeductionEntryQuery(string Department, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(RollingDeductionEntry item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<RollingDeductionEntry> Apply(List<RollingDeductionEntry> items)
    {
        var matched = Collections.Filter(items, (RollingDeductionEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling deduction entry store.</summary>
public static class RollingDeductionEntryStore
{
    public static IRollingDeductionEntryStore NewMemoryRollingDeductionEntryStore()
    {
        return new MemoryRollingDeductionEntryStore(
            new Dictionary<string, RollingDeductionEntry>(),
            new List<string>());
    }

    public static IRollingDeductionEntryStore NewCachedRollingDeductionEntryStore(MemoryRollingDeductionEntryStore inner, int ttl, IClock clock)
    {
        return new CachedRollingDeductionEntryStore(inner, RollingDeductionEntryCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RollingDeductionEntry> FindRollingDeductionEntries(IRollingDeductionEntryStore store, RollingDeductionEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
