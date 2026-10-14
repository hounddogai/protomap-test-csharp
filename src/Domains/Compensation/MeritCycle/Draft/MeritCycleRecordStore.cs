namespace Acme.Workforce.Compensation.MeritCycle.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle records by code.</summary>
public interface IMeritCycleRecordStore
{
    MeritCycleRecord? Find(string code);
    void Save(MeritCycleRecord item);
    bool Remove(string code);
    List<MeritCycleRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleRecordStore(Dictionary<string, MeritCycleRecord> items, List<string> order) : IMeritCycleRecordStore
{
    public Dictionary<string, MeritCycleRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleRecord item)
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

    public List<MeritCycleRecord> List()
    {
        var outValue = new List<MeritCycleRecord>();
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
public sealed class CachedMeritCycleRecordStore(MemoryMeritCycleRecordStore inner, MeritCycleRecordCache cache, int hits, int misses) : IMeritCycleRecordStore
{
    public MemoryMeritCycleRecordStore Inner { get; set; } = inner;
    public MeritCycleRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleRecord? Find(string code)
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

    public void Save(MeritCycleRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle records. Empty fields match all.</summary>
public sealed record MeritCycleRecordQuery(string Channel, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleRecord item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleRecord> Apply(List<MeritCycleRecord> items)
    {
        var matched = Collections.Filter(items, (MeritCycleRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle record store.</summary>
public static class MeritCycleRecordStore
{
    public static IMeritCycleRecordStore NewMemoryMeritCycleRecordStore()
    {
        return new MemoryMeritCycleRecordStore(new Dictionary<string, MeritCycleRecord>(), new List<string>());
    }

    public static IMeritCycleRecordStore NewCachedMeritCycleRecordStore(MemoryMeritCycleRecordStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleRecordStore(inner, MeritCycleRecordCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<MeritCycleRecord> FindMeritCycleRecords(IMeritCycleRecordStore store, MeritCycleRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
