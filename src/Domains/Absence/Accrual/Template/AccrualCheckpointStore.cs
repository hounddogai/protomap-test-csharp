namespace Acme.Workforce.Absence.Accrual.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual checkpoints by code.</summary>
public interface IAccrualCheckpointStore
{
    AccrualCheckpoint? Find(string code);
    void Save(AccrualCheckpoint item);
    bool Remove(string code);
    List<AccrualCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualCheckpointStore(Dictionary<string, AccrualCheckpoint> items, List<string> order) : IAccrualCheckpointStore
{
    public Dictionary<string, AccrualCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualCheckpoint item)
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

    public List<AccrualCheckpoint> List()
    {
        var outValue = new List<AccrualCheckpoint>();
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
public sealed class CachedAccrualCheckpointStore(MemoryAccrualCheckpointStore inner, AccrualCheckpointCache cache, int hits, int misses) : IAccrualCheckpointStore
{
    public MemoryAccrualCheckpointStore Inner { get; set; } = inner;
    public AccrualCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualCheckpoint? Find(string code)
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

    public void Save(AccrualCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual checkpoints. Empty fields match all.</summary>
public sealed record AccrualCheckpointQuery(string Title, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualCheckpoint item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccrualCheckpoint> Apply(List<AccrualCheckpoint> items)
    {
        var matched = Collections.Filter(items, (AccrualCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual checkpoint store.</summary>
public static class AccrualCheckpointStore
{
    public static IAccrualCheckpointStore NewMemoryAccrualCheckpointStore()
    {
        return new MemoryAccrualCheckpointStore(new Dictionary<string, AccrualCheckpoint>(), new List<string>());
    }

    public static IAccrualCheckpointStore NewCachedAccrualCheckpointStore(MemoryAccrualCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualCheckpointStore(inner, AccrualCheckpointCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AccrualCheckpoint> FindAccrualCheckpoints(IAccrualCheckpointStore store, AccrualCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
