namespace Acme.Workforce.Compliance.Exception.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback exception assignments by code.</summary>
public interface IFallbackExceptionAssignmentStore
{
    FallbackExceptionAssignment? Find(string code);
    void Save(FallbackExceptionAssignment item);
    bool Remove(string code);
    List<FallbackExceptionAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackExceptionAssignmentStore(Dictionary<string, FallbackExceptionAssignment> items, List<string> order) : IFallbackExceptionAssignmentStore
{
    public Dictionary<string, FallbackExceptionAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackExceptionAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackExceptionAssignment item)
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

    public List<FallbackExceptionAssignment> List()
    {
        var outValue = new List<FallbackExceptionAssignment>();
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
public sealed class CachedFallbackExceptionAssignmentStore(MemoryFallbackExceptionAssignmentStore inner, FallbackExceptionAssignmentCache cache, int hits, int misses) : IFallbackExceptionAssignmentStore
{
    public MemoryFallbackExceptionAssignmentStore Inner { get; set; } = inner;
    public FallbackExceptionAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackExceptionAssignment? Find(string code)
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

    public void Save(FallbackExceptionAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackExceptionAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback exception assignments. Empty fields match all.</summary>
public sealed record FallbackExceptionAssignmentQuery(string Name, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackExceptionAssignment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackExceptionAssignment> Apply(List<FallbackExceptionAssignment> items)
    {
        var matched = Collections.Filter(items, (FallbackExceptionAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback exception assignment store.</summary>
public static class FallbackExceptionAssignmentStore
{
    public static IFallbackExceptionAssignmentStore NewMemoryFallbackExceptionAssignmentStore()
    {
        return new MemoryFallbackExceptionAssignmentStore(
            new Dictionary<string, FallbackExceptionAssignment>(),
            new List<string>());
    }

    public static IFallbackExceptionAssignmentStore NewCachedFallbackExceptionAssignmentStore(MemoryFallbackExceptionAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackExceptionAssignmentStore(
            inner,
            FallbackExceptionAssignmentCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<FallbackExceptionAssignment> FindFallbackExceptionAssignments(IFallbackExceptionAssignmentStore store, FallbackExceptionAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
