namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work preferences by code.</summary>
public interface IStatementOfWorkPreferenceStore
{
    StatementOfWorkPreference? Find(string code);
    void Save(StatementOfWorkPreference item);
    bool Remove(string code);
    List<StatementOfWorkPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkPreferenceStore(Dictionary<string, StatementOfWorkPreference> items, List<string> order) : IStatementOfWorkPreferenceStore
{
    public Dictionary<string, StatementOfWorkPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkPreference item)
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

    public List<StatementOfWorkPreference> List()
    {
        var outValue = new List<StatementOfWorkPreference>();
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
public sealed class CachedStatementOfWorkPreferenceStore(MemoryStatementOfWorkPreferenceStore inner, StatementOfWorkPreferenceCache cache, int hits, int misses) : IStatementOfWorkPreferenceStore
{
    public MemoryStatementOfWorkPreferenceStore Inner { get; set; } = inner;
    public StatementOfWorkPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkPreference? Find(string code)
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

    public void Save(StatementOfWorkPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work preferences. Empty fields match all.</summary>
public sealed record StatementOfWorkPreferenceQuery(string Name, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkPreference item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkPreference> Apply(List<StatementOfWorkPreference> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work preference store.</summary>
public static class StatementOfWorkPreferenceStore
{
    public static IStatementOfWorkPreferenceStore NewMemoryStatementOfWorkPreferenceStore()
    {
        return new MemoryStatementOfWorkPreferenceStore(
            new Dictionary<string, StatementOfWorkPreference>(),
            new List<string>());
    }

    public static IStatementOfWorkPreferenceStore NewCachedStatementOfWorkPreferenceStore(MemoryStatementOfWorkPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkPreferenceStore(
            inner,
            StatementOfWorkPreferenceCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<StatementOfWorkPreference> FindStatementOfWorkPreferences(IStatementOfWorkPreferenceStore store, StatementOfWorkPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
