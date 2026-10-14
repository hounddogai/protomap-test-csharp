namespace Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending exception assignments by code.</summary>
public interface IPendingExceptionAssignmentStore
{
    PendingExceptionAssignment? Find(string code);
    void Save(PendingExceptionAssignment item);
    bool Remove(string code);
    List<PendingExceptionAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingExceptionAssignmentStore(Dictionary<string, PendingExceptionAssignment> items, List<string> order) : IPendingExceptionAssignmentStore
{
    public Dictionary<string, PendingExceptionAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingExceptionAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingExceptionAssignment item)
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

    public List<PendingExceptionAssignment> List()
    {
        var outValue = new List<PendingExceptionAssignment>();
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
public sealed class CachedPendingExceptionAssignmentStore(MemoryPendingExceptionAssignmentStore inner, PendingExceptionAssignmentCache cache, int hits, int misses) : IPendingExceptionAssignmentStore
{
    public MemoryPendingExceptionAssignmentStore Inner { get; set; } = inner;
    public PendingExceptionAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingExceptionAssignment? Find(string code)
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

    public void Save(PendingExceptionAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingExceptionAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending exception assignments. Empty fields match all.</summary>
public sealed record PendingExceptionAssignmentQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(PendingExceptionAssignment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingExceptionAssignment> Apply(List<PendingExceptionAssignment> items)
    {
        var matched = Collections.Filter(items, (PendingExceptionAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending exception assignment store.</summary>
public static class PendingExceptionAssignmentStore
{
    public static IPendingExceptionAssignmentStore NewMemoryPendingExceptionAssignmentStore()
    {
        return new MemoryPendingExceptionAssignmentStore(
            new Dictionary<string, PendingExceptionAssignment>(),
            new List<string>());
    }

    public static IPendingExceptionAssignmentStore NewCachedPendingExceptionAssignmentStore(MemoryPendingExceptionAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedPendingExceptionAssignmentStore(
            inner,
            PendingExceptionAssignmentCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PendingExceptionAssignment> FindPendingExceptionAssignments(IPendingExceptionAssignmentStore store, PendingExceptionAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
