namespace Acme.Workforce.Talent.SuccessionPlan.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary succession plan records by code.</summary>
public interface IPrimarySuccessionPlanRecordStore
{
    PrimarySuccessionPlanRecord? Find(string code);
    void Save(PrimarySuccessionPlanRecord item);
    bool Remove(string code);
    List<PrimarySuccessionPlanRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimarySuccessionPlanRecordStore(Dictionary<string, PrimarySuccessionPlanRecord> items, List<string> order) : IPrimarySuccessionPlanRecordStore
{
    public Dictionary<string, PrimarySuccessionPlanRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimarySuccessionPlanRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimarySuccessionPlanRecord item)
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

    public List<PrimarySuccessionPlanRecord> List()
    {
        var outValue = new List<PrimarySuccessionPlanRecord>();
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
public sealed class CachedPrimarySuccessionPlanRecordStore(MemoryPrimarySuccessionPlanRecordStore inner, PrimarySuccessionPlanRecordCache cache, int hits, int misses) : IPrimarySuccessionPlanRecordStore
{
    public MemoryPrimarySuccessionPlanRecordStore Inner { get; set; } = inner;
    public PrimarySuccessionPlanRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimarySuccessionPlanRecord? Find(string code)
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

    public void Save(PrimarySuccessionPlanRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimarySuccessionPlanRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary succession plan records. Empty fields match all.</summary>
public sealed record PrimarySuccessionPlanRecordQuery(string Region, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(PrimarySuccessionPlanRecord item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimarySuccessionPlanRecord> Apply(List<PrimarySuccessionPlanRecord> items)
    {
        var matched = Collections.Filter(items, (PrimarySuccessionPlanRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary succession plan record store.</summary>
public static class PrimarySuccessionPlanRecordStore
{
    public static IPrimarySuccessionPlanRecordStore NewMemoryPrimarySuccessionPlanRecordStore()
    {
        return new MemoryPrimarySuccessionPlanRecordStore(
            new Dictionary<string, PrimarySuccessionPlanRecord>(),
            new List<string>());
    }

    public static IPrimarySuccessionPlanRecordStore NewCachedPrimarySuccessionPlanRecordStore(MemoryPrimarySuccessionPlanRecordStore inner, int ttl, IClock clock)
    {
        return new CachedPrimarySuccessionPlanRecordStore(
            inner,
            PrimarySuccessionPlanRecordCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PrimarySuccessionPlanRecord> FindPrimarySuccessionPlanRecords(IPrimarySuccessionPlanRecordStore store, PrimarySuccessionPlanRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
