namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction entries by code.</summary>
public interface IDeductionEntryStore
{
    DeductionEntry? Find(string code);
    void Save(DeductionEntry item);
    bool Remove(string code);
    List<DeductionEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionEntryStore(Dictionary<string, DeductionEntry> items, List<string> order) : IDeductionEntryStore
{
    public Dictionary<string, DeductionEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionEntry item)
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

    public List<DeductionEntry> List()
    {
        var outValue = new List<DeductionEntry>();
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
public sealed class CachedDeductionEntryStore(MemoryDeductionEntryStore inner, DeductionEntryCache cache, int hits, int misses) : IDeductionEntryStore
{
    public MemoryDeductionEntryStore Inner { get; set; } = inner;
    public DeductionEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionEntry? Find(string code)
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

    public void Save(DeductionEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction entries. Empty fields match all.</summary>
public sealed record DeductionEntryQuery(string Channel, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionEntry item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeductionEntry> Apply(List<DeductionEntry> items)
    {
        var matched = Collections.Filter(items, (DeductionEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction entry store.</summary>
public static class DeductionEntryStore
{
    public static IDeductionEntryStore NewMemoryDeductionEntryStore()
    {
        return new MemoryDeductionEntryStore(new Dictionary<string, DeductionEntry>(), new List<string>());
    }

    public static IDeductionEntryStore NewCachedDeductionEntryStore(MemoryDeductionEntryStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionEntryStore(inner, DeductionEntryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<DeductionEntry> FindDeductionEntries(IDeductionEntryStore store, DeductionEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
