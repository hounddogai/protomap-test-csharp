namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work manifests by code.</summary>
public interface IStatementOfWorkManifestStore
{
    StatementOfWorkManifest? Find(string code);
    void Save(StatementOfWorkManifest item);
    bool Remove(string code);
    List<StatementOfWorkManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkManifestStore(Dictionary<string, StatementOfWorkManifest> items, List<string> order) : IStatementOfWorkManifestStore
{
    public Dictionary<string, StatementOfWorkManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkManifest item)
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

    public List<StatementOfWorkManifest> List()
    {
        var outValue = new List<StatementOfWorkManifest>();
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
public sealed class CachedStatementOfWorkManifestStore(MemoryStatementOfWorkManifestStore inner, StatementOfWorkManifestCache cache, int hits, int misses) : IStatementOfWorkManifestStore
{
    public MemoryStatementOfWorkManifestStore Inner { get; set; } = inner;
    public StatementOfWorkManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkManifest? Find(string code)
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

    public void Save(StatementOfWorkManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work manifests. Empty fields match all.</summary>
public sealed record StatementOfWorkManifestQuery(string Source, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkManifest item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkManifest> Apply(List<StatementOfWorkManifest> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work manifest store.</summary>
public static class StatementOfWorkManifestStore
{
    public static IStatementOfWorkManifestStore NewMemoryStatementOfWorkManifestStore()
    {
        return new MemoryStatementOfWorkManifestStore(
            new Dictionary<string, StatementOfWorkManifest>(),
            new List<string>());
    }

    public static IStatementOfWorkManifestStore NewCachedStatementOfWorkManifestStore(MemoryStatementOfWorkManifestStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkManifestStore(
            inner,
            StatementOfWorkManifestCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StatementOfWorkManifest> FindStatementOfWorkManifests(IStatementOfWorkManifestStore store, StatementOfWorkManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
