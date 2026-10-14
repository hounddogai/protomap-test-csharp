namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling merit cycle records by code.</summary>
public interface IRollingMeritCycleRecordStore
{
    RollingMeritCycleRecord? Find(string code);
    void Save(RollingMeritCycleRecord item);
    bool Remove(string code);
    List<RollingMeritCycleRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingMeritCycleRecordStore(Dictionary<string, RollingMeritCycleRecord> items, List<string> order) : IRollingMeritCycleRecordStore
{
    public Dictionary<string, RollingMeritCycleRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingMeritCycleRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingMeritCycleRecord item)
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

    public List<RollingMeritCycleRecord> List()
    {
        var outValue = new List<RollingMeritCycleRecord>();
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
public sealed class CachedRollingMeritCycleRecordStore(MemoryRollingMeritCycleRecordStore inner, RollingMeritCycleRecordCache cache, int hits, int misses) : IRollingMeritCycleRecordStore
{
    public MemoryRollingMeritCycleRecordStore Inner { get; set; } = inner;
    public RollingMeritCycleRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingMeritCycleRecord? Find(string code)
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

    public void Save(RollingMeritCycleRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingMeritCycleRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling merit cycle records. Empty fields match all.</summary>
public sealed record RollingMeritCycleRecordQuery(string Title, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(RollingMeritCycleRecord item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingMeritCycleRecord> Apply(List<RollingMeritCycleRecord> items)
    {
        var matched = Collections.Filter(items, (RollingMeritCycleRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling merit cycle record store.</summary>
public static class RollingMeritCycleRecordStore
{
    public static IRollingMeritCycleRecordStore NewMemoryRollingMeritCycleRecordStore()
    {
        return new MemoryRollingMeritCycleRecordStore(
            new Dictionary<string, RollingMeritCycleRecord>(),
            new List<string>());
    }

    public static IRollingMeritCycleRecordStore NewCachedRollingMeritCycleRecordStore(MemoryRollingMeritCycleRecordStore inner, int ttl, IClock clock)
    {
        return new CachedRollingMeritCycleRecordStore(
            inner,
            RollingMeritCycleRecordCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<RollingMeritCycleRecord> FindRollingMeritCycleRecords(IRollingMeritCycleRecordStore store, RollingMeritCycleRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
