namespace Acme.Workforce.Compliance.Exception.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception assignments by code.</summary>
public interface IExceptionAssignmentStore
{
    ExceptionAssignment? Find(string code);
    void Save(ExceptionAssignment item);
    bool Remove(string code);
    List<ExceptionAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionAssignmentStore(Dictionary<string, ExceptionAssignment> items, List<string> order) : IExceptionAssignmentStore
{
    public Dictionary<string, ExceptionAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionAssignment item)
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

    public List<ExceptionAssignment> List()
    {
        var outValue = new List<ExceptionAssignment>();
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
public sealed class CachedExceptionAssignmentStore(MemoryExceptionAssignmentStore inner, ExceptionAssignmentCache cache, int hits, int misses) : IExceptionAssignmentStore
{
    public MemoryExceptionAssignmentStore Inner { get; set; } = inner;
    public ExceptionAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionAssignment? Find(string code)
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

    public void Save(ExceptionAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception assignments. Empty fields match all.</summary>
public sealed record ExceptionAssignmentQuery(string Locale, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionAssignment item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ExceptionAssignment> Apply(List<ExceptionAssignment> items)
    {
        var matched = Collections.Filter(items, (ExceptionAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception assignment store.</summary>
public static class ExceptionAssignmentStore
{
    public static IExceptionAssignmentStore NewMemoryExceptionAssignmentStore()
    {
        return new MemoryExceptionAssignmentStore(new Dictionary<string, ExceptionAssignment>(), new List<string>());
    }

    public static IExceptionAssignmentStore NewCachedExceptionAssignmentStore(MemoryExceptionAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionAssignmentStore(inner, ExceptionAssignmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ExceptionAssignment> FindExceptionAssignments(IExceptionAssignmentStore store, ExceptionAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
