namespace Acme.Workforce.Orgdesign.Position.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy position ledgers by code.</summary>
public interface ILegacyPositionLedgerStore
{
    LegacyPositionLedger? Find(string code);
    void Save(LegacyPositionLedger item);
    bool Remove(string code);
    List<LegacyPositionLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyPositionLedgerStore(Dictionary<string, LegacyPositionLedger> items, List<string> order) : ILegacyPositionLedgerStore
{
    public Dictionary<string, LegacyPositionLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyPositionLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyPositionLedger item)
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

    public List<LegacyPositionLedger> List()
    {
        var outValue = new List<LegacyPositionLedger>();
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
public sealed class CachedLegacyPositionLedgerStore(MemoryLegacyPositionLedgerStore inner, LegacyPositionLedgerCache cache, int hits, int misses) : ILegacyPositionLedgerStore
{
    public MemoryLegacyPositionLedgerStore Inner { get; set; } = inner;
    public LegacyPositionLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyPositionLedger? Find(string code)
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

    public void Save(LegacyPositionLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyPositionLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy position ledgers. Empty fields match all.</summary>
public sealed record LegacyPositionLedgerQuery(string Department, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyPositionLedger item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyPositionLedger> Apply(List<LegacyPositionLedger> items)
    {
        var matched = Collections.Filter(items, (LegacyPositionLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy position ledger store.</summary>
public static class LegacyPositionLedgerStore
{
    public static ILegacyPositionLedgerStore NewMemoryLegacyPositionLedgerStore()
    {
        return new MemoryLegacyPositionLedgerStore(
            new Dictionary<string, LegacyPositionLedger>(),
            new List<string>());
    }

    public static ILegacyPositionLedgerStore NewCachedLegacyPositionLedgerStore(MemoryLegacyPositionLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyPositionLedgerStore(inner, LegacyPositionLedgerCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LegacyPositionLedger> FindLegacyPositionLedgers(ILegacyPositionLedgerStore store, LegacyPositionLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
