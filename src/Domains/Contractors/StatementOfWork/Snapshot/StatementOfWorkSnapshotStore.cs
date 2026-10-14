namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work snapshots by code.</summary>
public interface IStatementOfWorkSnapshotStore
{
    StatementOfWorkSnapshot? Find(string code);
    void Save(StatementOfWorkSnapshot item);
    bool Remove(string code);
    List<StatementOfWorkSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkSnapshotStore(Dictionary<string, StatementOfWorkSnapshot> items, List<string> order) : IStatementOfWorkSnapshotStore
{
    public Dictionary<string, StatementOfWorkSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkSnapshot item)
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

    public List<StatementOfWorkSnapshot> List()
    {
        var outValue = new List<StatementOfWorkSnapshot>();
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
public sealed class CachedStatementOfWorkSnapshotStore(MemoryStatementOfWorkSnapshotStore inner, StatementOfWorkSnapshotCache cache, int hits, int misses) : IStatementOfWorkSnapshotStore
{
    public MemoryStatementOfWorkSnapshotStore Inner { get; set; } = inner;
    public StatementOfWorkSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkSnapshot? Find(string code)
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

    public void Save(StatementOfWorkSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work snapshots. Empty fields match all.</summary>
public sealed record StatementOfWorkSnapshotQuery(string Team, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkSnapshot item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkSnapshot> Apply(List<StatementOfWorkSnapshot> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work snapshot store.</summary>
public static class StatementOfWorkSnapshotStore
{
    public static IStatementOfWorkSnapshotStore NewMemoryStatementOfWorkSnapshotStore()
    {
        return new MemoryStatementOfWorkSnapshotStore(
            new Dictionary<string, StatementOfWorkSnapshot>(),
            new List<string>());
    }

    public static IStatementOfWorkSnapshotStore NewCachedStatementOfWorkSnapshotStore(MemoryStatementOfWorkSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkSnapshotStore(
            inner,
            StatementOfWorkSnapshotCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StatementOfWorkSnapshot> FindStatementOfWorkSnapshots(IStatementOfWorkSnapshotStore store, StatementOfWorkSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
