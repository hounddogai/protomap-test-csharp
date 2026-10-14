namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending visitor assignments by code.</summary>
public interface IPendingVisitorAssignmentStore
{
    PendingVisitorAssignment? Find(string code);
    void Save(PendingVisitorAssignment item);
    bool Remove(string code);
    List<PendingVisitorAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingVisitorAssignmentStore(Dictionary<string, PendingVisitorAssignment> items, List<string> order) : IPendingVisitorAssignmentStore
{
    public Dictionary<string, PendingVisitorAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingVisitorAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingVisitorAssignment item)
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

    public List<PendingVisitorAssignment> List()
    {
        var outValue = new List<PendingVisitorAssignment>();
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
public sealed class CachedPendingVisitorAssignmentStore(MemoryPendingVisitorAssignmentStore inner, PendingVisitorAssignmentCache cache, int hits, int misses) : IPendingVisitorAssignmentStore
{
    public MemoryPendingVisitorAssignmentStore Inner { get; set; } = inner;
    public PendingVisitorAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingVisitorAssignment? Find(string code)
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

    public void Save(PendingVisitorAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingVisitorAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending visitor assignments. Empty fields match all.</summary>
public sealed record PendingVisitorAssignmentQuery(string Region, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(PendingVisitorAssignment item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingVisitorAssignment> Apply(List<PendingVisitorAssignment> items)
    {
        var matched = Collections.Filter(items, (PendingVisitorAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending visitor assignment store.</summary>
public static class PendingVisitorAssignmentStore
{
    public static IPendingVisitorAssignmentStore NewMemoryPendingVisitorAssignmentStore()
    {
        return new MemoryPendingVisitorAssignmentStore(
            new Dictionary<string, PendingVisitorAssignment>(),
            new List<string>());
    }

    public static IPendingVisitorAssignmentStore NewCachedPendingVisitorAssignmentStore(MemoryPendingVisitorAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedPendingVisitorAssignmentStore(
            inner,
            PendingVisitorAssignmentCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PendingVisitorAssignment> FindPendingVisitorAssignments(IPendingVisitorAssignmentStore store, PendingVisitorAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
