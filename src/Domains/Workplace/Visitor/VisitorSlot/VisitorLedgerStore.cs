namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor ledgers by code.</summary>
public interface IVisitorLedgerStore
{
    VisitorLedger? Find(string code);
    void Save(VisitorLedger item);
    bool Remove(string code);
    List<VisitorLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorLedgerStore(Dictionary<string, VisitorLedger> items, List<string> order) : IVisitorLedgerStore
{
    public Dictionary<string, VisitorLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorLedger item)
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

    public List<VisitorLedger> List()
    {
        var outValue = new List<VisitorLedger>();
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
public sealed class CachedVisitorLedgerStore(MemoryVisitorLedgerStore inner, VisitorLedgerCache cache, int hits, int misses) : IVisitorLedgerStore
{
    public MemoryVisitorLedgerStore Inner { get; set; } = inner;
    public VisitorLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorLedger? Find(string code)
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

    public void Save(VisitorLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor ledgers. Empty fields match all.</summary>
public sealed record VisitorLedgerQuery(string Title, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorLedger item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorLedger> Apply(List<VisitorLedger> items)
    {
        var matched = Collections.Filter(items, (VisitorLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor ledger store.</summary>
public static class VisitorLedgerStore
{
    public static IVisitorLedgerStore NewMemoryVisitorLedgerStore()
    {
        return new MemoryVisitorLedgerStore(new Dictionary<string, VisitorLedger>(), new List<string>());
    }

    public static IVisitorLedgerStore NewCachedVisitorLedgerStore(MemoryVisitorLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorLedgerStore(inner, VisitorLedgerCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<VisitorLedger> FindVisitorLedgers(IVisitorLedgerStore store, VisitorLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
