namespace Acme.Workforce.Contractors.StatementOfWork.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work windows by code.</summary>
public interface IStatementOfWorkWindowStore
{
    StatementOfWorkWindow? Find(string code);
    void Save(StatementOfWorkWindow item);
    bool Remove(string code);
    List<StatementOfWorkWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkWindowStore(Dictionary<string, StatementOfWorkWindow> items, List<string> order) : IStatementOfWorkWindowStore
{
    public Dictionary<string, StatementOfWorkWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkWindow item)
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

    public List<StatementOfWorkWindow> List()
    {
        var outValue = new List<StatementOfWorkWindow>();
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
public sealed class CachedStatementOfWorkWindowStore(MemoryStatementOfWorkWindowStore inner, StatementOfWorkWindowCache cache, int hits, int misses) : IStatementOfWorkWindowStore
{
    public MemoryStatementOfWorkWindowStore Inner { get; set; } = inner;
    public StatementOfWorkWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkWindow? Find(string code)
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

    public void Save(StatementOfWorkWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work windows. Empty fields match all.</summary>
public sealed record StatementOfWorkWindowQuery(string Channel, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkWindow item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkWindow> Apply(List<StatementOfWorkWindow> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work window store.</summary>
public static class StatementOfWorkWindowStore
{
    public static IStatementOfWorkWindowStore NewMemoryStatementOfWorkWindowStore()
    {
        return new MemoryStatementOfWorkWindowStore(
            new Dictionary<string, StatementOfWorkWindow>(),
            new List<string>());
    }

    public static IStatementOfWorkWindowStore NewCachedStatementOfWorkWindowStore(MemoryStatementOfWorkWindowStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkWindowStore(inner, StatementOfWorkWindowCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<StatementOfWorkWindow> FindStatementOfWorkWindows(IStatementOfWorkWindowStore store, StatementOfWorkWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
