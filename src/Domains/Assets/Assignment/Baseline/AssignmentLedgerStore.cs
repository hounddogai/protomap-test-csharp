namespace Acme.Workforce.Assets.Assignment.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment ledgers by code.</summary>
public interface IAssignmentLedgerStore
{
    AssignmentLedger? Find(string code);
    void Save(AssignmentLedger item);
    bool Remove(string code);
    List<AssignmentLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentLedgerStore(Dictionary<string, AssignmentLedger> items, List<string> order) : IAssignmentLedgerStore
{
    public Dictionary<string, AssignmentLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentLedger item)
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

    public List<AssignmentLedger> List()
    {
        var outValue = new List<AssignmentLedger>();
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
public sealed class CachedAssignmentLedgerStore(MemoryAssignmentLedgerStore inner, AssignmentLedgerCache cache, int hits, int misses) : IAssignmentLedgerStore
{
    public MemoryAssignmentLedgerStore Inner { get; set; } = inner;
    public AssignmentLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentLedger? Find(string code)
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

    public void Save(AssignmentLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment ledgers. Empty fields match all.</summary>
public sealed record AssignmentLedgerQuery(string Locale, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentLedger item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentLedger> Apply(List<AssignmentLedger> items)
    {
        var matched = Collections.Filter(items, (AssignmentLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment ledger store.</summary>
public static class AssignmentLedgerStore
{
    public static IAssignmentLedgerStore NewMemoryAssignmentLedgerStore()
    {
        return new MemoryAssignmentLedgerStore(new Dictionary<string, AssignmentLedger>(), new List<string>());
    }

    public static IAssignmentLedgerStore NewCachedAssignmentLedgerStore(MemoryAssignmentLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentLedgerStore(inner, AssignmentLedgerCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AssignmentLedger> FindAssignmentLedgers(IAssignmentLedgerStore store, AssignmentLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
