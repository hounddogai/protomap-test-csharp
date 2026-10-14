namespace Acme.Workforce.Assets.Assignment.AssignmentEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending assignment entries by code.</summary>
public interface IPendingAssignmentEntryStore
{
    PendingAssignmentEntry? Find(string code);
    void Save(PendingAssignmentEntry item);
    bool Remove(string code);
    List<PendingAssignmentEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingAssignmentEntryStore(Dictionary<string, PendingAssignmentEntry> items, List<string> order) : IPendingAssignmentEntryStore
{
    public Dictionary<string, PendingAssignmentEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingAssignmentEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingAssignmentEntry item)
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

    public List<PendingAssignmentEntry> List()
    {
        var outValue = new List<PendingAssignmentEntry>();
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
public sealed class CachedPendingAssignmentEntryStore(MemoryPendingAssignmentEntryStore inner, PendingAssignmentEntryCache cache, int hits, int misses) : IPendingAssignmentEntryStore
{
    public MemoryPendingAssignmentEntryStore Inner { get; set; } = inner;
    public PendingAssignmentEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingAssignmentEntry? Find(string code)
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

    public void Save(PendingAssignmentEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingAssignmentEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending assignment entries. Empty fields match all.</summary>
public sealed record PendingAssignmentEntryQuery(string Name, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(PendingAssignmentEntry item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<PendingAssignmentEntry> Apply(List<PendingAssignmentEntry> items)
    {
        var matched = Collections.Filter(items, (PendingAssignmentEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending assignment entry store.</summary>
public static class PendingAssignmentEntryStore
{
    public static IPendingAssignmentEntryStore NewMemoryPendingAssignmentEntryStore()
    {
        return new MemoryPendingAssignmentEntryStore(
            new Dictionary<string, PendingAssignmentEntry>(),
            new List<string>());
    }

    public static IPendingAssignmentEntryStore NewCachedPendingAssignmentEntryStore(MemoryPendingAssignmentEntryStore inner, int ttl, IClock clock)
    {
        return new CachedPendingAssignmentEntryStore(
            inner,
            PendingAssignmentEntryCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PendingAssignmentEntry> FindPendingAssignmentEntries(IPendingAssignmentEntryStore store, PendingAssignmentEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
