namespace Acme.Workforce.Payrollops.Deduction.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient deduction entries by code.</summary>
public interface ILenientDeductionEntryStore
{
    LenientDeductionEntry? Find(string code);
    void Save(LenientDeductionEntry item);
    bool Remove(string code);
    List<LenientDeductionEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientDeductionEntryStore(Dictionary<string, LenientDeductionEntry> items, List<string> order) : ILenientDeductionEntryStore
{
    public Dictionary<string, LenientDeductionEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientDeductionEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientDeductionEntry item)
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

    public List<LenientDeductionEntry> List()
    {
        var outValue = new List<LenientDeductionEntry>();
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
public sealed class CachedLenientDeductionEntryStore(MemoryLenientDeductionEntryStore inner, LenientDeductionEntryCache cache, int hits, int misses) : ILenientDeductionEntryStore
{
    public MemoryLenientDeductionEntryStore Inner { get; set; } = inner;
    public LenientDeductionEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientDeductionEntry? Find(string code)
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

    public void Save(LenientDeductionEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientDeductionEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient deduction entries. Empty fields match all.</summary>
public sealed record LenientDeductionEntryQuery(string Team, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(LenientDeductionEntry item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<LenientDeductionEntry> Apply(List<LenientDeductionEntry> items)
    {
        var matched = Collections.Filter(items, (LenientDeductionEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient deduction entry store.</summary>
public static class LenientDeductionEntryStore
{
    public static ILenientDeductionEntryStore NewMemoryLenientDeductionEntryStore()
    {
        return new MemoryLenientDeductionEntryStore(
            new Dictionary<string, LenientDeductionEntry>(),
            new List<string>());
    }

    public static ILenientDeductionEntryStore NewCachedLenientDeductionEntryStore(MemoryLenientDeductionEntryStore inner, int ttl, IClock clock)
    {
        return new CachedLenientDeductionEntryStore(inner, LenientDeductionEntryCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LenientDeductionEntry> FindLenientDeductionEntries(ILenientDeductionEntryStore store, LenientDeductionEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
