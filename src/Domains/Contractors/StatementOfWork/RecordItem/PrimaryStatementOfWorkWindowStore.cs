namespace Acme.Workforce.Contractors.StatementOfWork.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary statement of work windows by code.</summary>
public interface IPrimaryStatementOfWorkWindowStore
{
    PrimaryStatementOfWorkWindow? Find(string code);
    void Save(PrimaryStatementOfWorkWindow item);
    bool Remove(string code);
    List<PrimaryStatementOfWorkWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryStatementOfWorkWindowStore(Dictionary<string, PrimaryStatementOfWorkWindow> items, List<string> order) : IPrimaryStatementOfWorkWindowStore
{
    public Dictionary<string, PrimaryStatementOfWorkWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryStatementOfWorkWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryStatementOfWorkWindow item)
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

    public List<PrimaryStatementOfWorkWindow> List()
    {
        var outValue = new List<PrimaryStatementOfWorkWindow>();
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
public sealed class CachedPrimaryStatementOfWorkWindowStore(MemoryPrimaryStatementOfWorkWindowStore inner, PrimaryStatementOfWorkWindowCache cache, int hits, int misses) : IPrimaryStatementOfWorkWindowStore
{
    public MemoryPrimaryStatementOfWorkWindowStore Inner { get; set; } = inner;
    public PrimaryStatementOfWorkWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryStatementOfWorkWindow? Find(string code)
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

    public void Save(PrimaryStatementOfWorkWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryStatementOfWorkWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary statement of work windows. Empty fields match all.</summary>
public sealed record PrimaryStatementOfWorkWindowQuery(string Source, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryStatementOfWorkWindow item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryStatementOfWorkWindow> Apply(List<PrimaryStatementOfWorkWindow> items)
    {
        var matched = Collections.Filter(items, (PrimaryStatementOfWorkWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary statement of work window store.</summary>
public static class PrimaryStatementOfWorkWindowStore
{
    public static IPrimaryStatementOfWorkWindowStore NewMemoryPrimaryStatementOfWorkWindowStore()
    {
        return new MemoryPrimaryStatementOfWorkWindowStore(
            new Dictionary<string, PrimaryStatementOfWorkWindow>(),
            new List<string>());
    }

    public static IPrimaryStatementOfWorkWindowStore NewCachedPrimaryStatementOfWorkWindowStore(MemoryPrimaryStatementOfWorkWindowStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryStatementOfWorkWindowStore(
            inner,
            PrimaryStatementOfWorkWindowCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryStatementOfWorkWindow> FindPrimaryStatementOfWorkWindows(IPrimaryStatementOfWorkWindowStore store, PrimaryStatementOfWorkWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
