namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan records by code.</summary>
public interface IActionPlanRecordStore
{
    ActionPlanRecord? Find(string code);
    void Save(ActionPlanRecord item);
    bool Remove(string code);
    List<ActionPlanRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanRecordStore(Dictionary<string, ActionPlanRecord> items, List<string> order) : IActionPlanRecordStore
{
    public Dictionary<string, ActionPlanRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanRecord item)
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

    public List<ActionPlanRecord> List()
    {
        var outValue = new List<ActionPlanRecord>();
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
public sealed class CachedActionPlanRecordStore(MemoryActionPlanRecordStore inner, ActionPlanRecordCache cache, int hits, int misses) : IActionPlanRecordStore
{
    public MemoryActionPlanRecordStore Inner { get; set; } = inner;
    public ActionPlanRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanRecord? Find(string code)
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

    public void Save(ActionPlanRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan records. Empty fields match all.</summary>
public sealed record ActionPlanRecordQuery(string Name, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanRecord item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ActionPlanRecord> Apply(List<ActionPlanRecord> items)
    {
        var matched = Collections.Filter(items, (ActionPlanRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan record store.</summary>
public static class ActionPlanRecordStore
{
    public static IActionPlanRecordStore NewMemoryActionPlanRecordStore()
    {
        return new MemoryActionPlanRecordStore(new Dictionary<string, ActionPlanRecord>(), new List<string>());
    }

    public static IActionPlanRecordStore NewCachedActionPlanRecordStore(MemoryActionPlanRecordStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanRecordStore(inner, ActionPlanRecordCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ActionPlanRecord> FindActionPlanRecords(IActionPlanRecordStore store, ActionPlanRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
