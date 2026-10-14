namespace Acme.Workforce.Engagement.ActionPlan.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual action plan records by code.</summary>
public interface IManualActionPlanRecordStore
{
    ManualActionPlanRecord? Find(string code);
    void Save(ManualActionPlanRecord item);
    bool Remove(string code);
    List<ManualActionPlanRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualActionPlanRecordStore(Dictionary<string, ManualActionPlanRecord> items, List<string> order) : IManualActionPlanRecordStore
{
    public Dictionary<string, ManualActionPlanRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualActionPlanRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualActionPlanRecord item)
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

    public List<ManualActionPlanRecord> List()
    {
        var outValue = new List<ManualActionPlanRecord>();
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
public sealed class CachedManualActionPlanRecordStore(MemoryManualActionPlanRecordStore inner, ManualActionPlanRecordCache cache, int hits, int misses) : IManualActionPlanRecordStore
{
    public MemoryManualActionPlanRecordStore Inner { get; set; } = inner;
    public ManualActionPlanRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualActionPlanRecord? Find(string code)
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

    public void Save(ManualActionPlanRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualActionPlanRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual action plan records. Empty fields match all.</summary>
public sealed record ManualActionPlanRecordQuery(string Category, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(ManualActionPlanRecord item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<ManualActionPlanRecord> Apply(List<ManualActionPlanRecord> items)
    {
        var matched = Collections.Filter(items, (ManualActionPlanRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual action plan record store.</summary>
public static class ManualActionPlanRecordStore
{
    public static IManualActionPlanRecordStore NewMemoryManualActionPlanRecordStore()
    {
        return new MemoryManualActionPlanRecordStore(
            new Dictionary<string, ManualActionPlanRecord>(),
            new List<string>());
    }

    public static IManualActionPlanRecordStore NewCachedManualActionPlanRecordStore(MemoryManualActionPlanRecordStore inner, int ttl, IClock clock)
    {
        return new CachedManualActionPlanRecordStore(
            inner,
            ManualActionPlanRecordCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ManualActionPlanRecord> FindManualActionPlanRecords(IManualActionPlanRecordStore store, ManualActionPlanRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
