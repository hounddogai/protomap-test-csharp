namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment entries by code.</summary>
public interface IAssignmentEntryStore
{
    AssignmentEntry? Find(string code);
    void Save(AssignmentEntry item);
    bool Remove(string code);
    List<AssignmentEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentEntryStore(Dictionary<string, AssignmentEntry> items, List<string> order) : IAssignmentEntryStore
{
    public Dictionary<string, AssignmentEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentEntry item)
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

    public List<AssignmentEntry> List()
    {
        var outValue = new List<AssignmentEntry>();
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
public sealed class CachedAssignmentEntryStore(MemoryAssignmentEntryStore inner, AssignmentEntryCache cache, int hits, int misses) : IAssignmentEntryStore
{
    public MemoryAssignmentEntryStore Inner { get; set; } = inner;
    public AssignmentEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentEntry? Find(string code)
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

    public void Save(AssignmentEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment entries. Empty fields match all.</summary>
public sealed record AssignmentEntryQuery(string Unit, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentEntry item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentEntry> Apply(List<AssignmentEntry> items)
    {
        var matched = Collections.Filter(items, (AssignmentEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment entry store.</summary>
public static class AssignmentEntryStore
{
    public static IAssignmentEntryStore NewMemoryAssignmentEntryStore()
    {
        return new MemoryAssignmentEntryStore(new Dictionary<string, AssignmentEntry>(), new List<string>());
    }

    public static IAssignmentEntryStore NewCachedAssignmentEntryStore(MemoryAssignmentEntryStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentEntryStore(inner, AssignmentEntryCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AssignmentEntry> FindAssignmentEntries(IAssignmentEntryStore store, AssignmentEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
