namespace Acme.Workforce.Contractors.StatementOfWork.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work groups by code.</summary>
public interface IStatementOfWorkGroupStore
{
    StatementOfWorkGroup? Find(string code);
    void Save(StatementOfWorkGroup item);
    bool Remove(string code);
    List<StatementOfWorkGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkGroupStore(Dictionary<string, StatementOfWorkGroup> items, List<string> order) : IStatementOfWorkGroupStore
{
    public Dictionary<string, StatementOfWorkGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkGroup item)
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

    public List<StatementOfWorkGroup> List()
    {
        var outValue = new List<StatementOfWorkGroup>();
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
public sealed class CachedStatementOfWorkGroupStore(MemoryStatementOfWorkGroupStore inner, StatementOfWorkGroupCache cache, int hits, int misses) : IStatementOfWorkGroupStore
{
    public MemoryStatementOfWorkGroupStore Inner { get; set; } = inner;
    public StatementOfWorkGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkGroup? Find(string code)
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

    public void Save(StatementOfWorkGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work groups. Empty fields match all.</summary>
public sealed record StatementOfWorkGroupQuery(string Unit, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkGroup item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<StatementOfWorkGroup> Apply(List<StatementOfWorkGroup> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work group store.</summary>
public static class StatementOfWorkGroupStore
{
    public static IStatementOfWorkGroupStore NewMemoryStatementOfWorkGroupStore()
    {
        return new MemoryStatementOfWorkGroupStore(
            new Dictionary<string, StatementOfWorkGroup>(),
            new List<string>());
    }

    public static IStatementOfWorkGroupStore NewCachedStatementOfWorkGroupStore(MemoryStatementOfWorkGroupStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkGroupStore(inner, StatementOfWorkGroupCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<StatementOfWorkGroup> FindStatementOfWorkGroups(IStatementOfWorkGroupStore store, StatementOfWorkGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
