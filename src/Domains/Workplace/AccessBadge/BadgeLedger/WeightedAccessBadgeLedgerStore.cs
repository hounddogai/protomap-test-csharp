namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted access badge ledgers by code.</summary>
public interface IWeightedAccessBadgeLedgerStore
{
    WeightedAccessBadgeLedger? Find(string code);
    void Save(WeightedAccessBadgeLedger item);
    bool Remove(string code);
    List<WeightedAccessBadgeLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedAccessBadgeLedgerStore(Dictionary<string, WeightedAccessBadgeLedger> items, List<string> order) : IWeightedAccessBadgeLedgerStore
{
    public Dictionary<string, WeightedAccessBadgeLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedAccessBadgeLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedAccessBadgeLedger item)
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

    public List<WeightedAccessBadgeLedger> List()
    {
        var outValue = new List<WeightedAccessBadgeLedger>();
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
public sealed class CachedWeightedAccessBadgeLedgerStore(MemoryWeightedAccessBadgeLedgerStore inner, WeightedAccessBadgeLedgerCache cache, int hits, int misses) : IWeightedAccessBadgeLedgerStore
{
    public MemoryWeightedAccessBadgeLedgerStore Inner { get; set; } = inner;
    public WeightedAccessBadgeLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedAccessBadgeLedger? Find(string code)
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

    public void Save(WeightedAccessBadgeLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedAccessBadgeLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted access badge ledgers. Empty fields match all.</summary>
public sealed record WeightedAccessBadgeLedgerQuery(string Name, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedAccessBadgeLedger item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedAccessBadgeLedger> Apply(List<WeightedAccessBadgeLedger> items)
    {
        var matched = Collections.Filter(items, (WeightedAccessBadgeLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted access badge ledger store.</summary>
public static class WeightedAccessBadgeLedgerStore
{
    public static IWeightedAccessBadgeLedgerStore NewMemoryWeightedAccessBadgeLedgerStore()
    {
        return new MemoryWeightedAccessBadgeLedgerStore(
            new Dictionary<string, WeightedAccessBadgeLedger>(),
            new List<string>());
    }

    public static IWeightedAccessBadgeLedgerStore NewCachedWeightedAccessBadgeLedgerStore(MemoryWeightedAccessBadgeLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedAccessBadgeLedgerStore(
            inner,
            WeightedAccessBadgeLedgerCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<WeightedAccessBadgeLedger> FindWeightedAccessBadgeLedgers(IWeightedAccessBadgeLedgerStore store, WeightedAccessBadgeLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
