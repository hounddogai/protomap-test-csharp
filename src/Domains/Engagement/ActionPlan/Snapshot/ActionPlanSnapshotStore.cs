namespace Acme.Workforce.Engagement.ActionPlan.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan snapshots by code.</summary>
public interface IActionPlanSnapshotStore
{
    ActionPlanSnapshot? Find(string code);
    void Save(ActionPlanSnapshot item);
    bool Remove(string code);
    List<ActionPlanSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanSnapshotStore(Dictionary<string, ActionPlanSnapshot> items, List<string> order) : IActionPlanSnapshotStore
{
    public Dictionary<string, ActionPlanSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanSnapshot item)
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

    public List<ActionPlanSnapshot> List()
    {
        var outValue = new List<ActionPlanSnapshot>();
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
public sealed class CachedActionPlanSnapshotStore(MemoryActionPlanSnapshotStore inner, ActionPlanSnapshotCache cache, int hits, int misses) : IActionPlanSnapshotStore
{
    public MemoryActionPlanSnapshotStore Inner { get; set; } = inner;
    public ActionPlanSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanSnapshot? Find(string code)
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

    public void Save(ActionPlanSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan snapshots. Empty fields match all.</summary>
public sealed record ActionPlanSnapshotQuery(string Locale, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanSnapshot item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ActionPlanSnapshot> Apply(List<ActionPlanSnapshot> items)
    {
        var matched = Collections.Filter(items, (ActionPlanSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan snapshot store.</summary>
public static class ActionPlanSnapshotStore
{
    public static IActionPlanSnapshotStore NewMemoryActionPlanSnapshotStore()
    {
        return new MemoryActionPlanSnapshotStore(new Dictionary<string, ActionPlanSnapshot>(), new List<string>());
    }

    public static IActionPlanSnapshotStore NewCachedActionPlanSnapshotStore(MemoryActionPlanSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanSnapshotStore(inner, ActionPlanSnapshotCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ActionPlanSnapshot> FindActionPlanSnapshots(IActionPlanSnapshotStore store, ActionPlanSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
