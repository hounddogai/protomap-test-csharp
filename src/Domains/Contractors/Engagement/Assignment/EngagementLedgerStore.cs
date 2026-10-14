namespace Acme.Workforce.Contractors.Engagement.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps engagement ledgers by code.</summary>
public interface IEngagementLedgerStore
{
    EngagementLedger? Find(string code);
    void Save(EngagementLedger item);
    bool Remove(string code);
    List<EngagementLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEngagementLedgerStore(Dictionary<string, EngagementLedger> items, List<string> order) : IEngagementLedgerStore
{
    public Dictionary<string, EngagementLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EngagementLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EngagementLedger item)
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

    public List<EngagementLedger> List()
    {
        var outValue = new List<EngagementLedger>();
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
public sealed class CachedEngagementLedgerStore(MemoryEngagementLedgerStore inner, EngagementLedgerCache cache, int hits, int misses) : IEngagementLedgerStore
{
    public MemoryEngagementLedgerStore Inner { get; set; } = inner;
    public EngagementLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EngagementLedger? Find(string code)
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

    public void Save(EngagementLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EngagementLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects engagement ledgers. Empty fields match all.</summary>
public sealed record EngagementLedgerQuery(string Label, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(EngagementLedger item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EngagementLedger> Apply(List<EngagementLedger> items)
    {
        var matched = Collections.Filter(items, (EngagementLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of engagement ledger store.</summary>
public static class EngagementLedgerStore
{
    public static IEngagementLedgerStore NewMemoryEngagementLedgerStore()
    {
        return new MemoryEngagementLedgerStore(new Dictionary<string, EngagementLedger>(), new List<string>());
    }

    public static IEngagementLedgerStore NewCachedEngagementLedgerStore(MemoryEngagementLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedEngagementLedgerStore(inner, EngagementLedgerCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<EngagementLedger> FindEngagementLedgers(IEngagementLedgerStore store, EngagementLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
