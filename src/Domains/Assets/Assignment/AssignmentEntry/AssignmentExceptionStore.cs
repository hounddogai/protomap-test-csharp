namespace Acme.Workforce.Assets.Assignment.AssignmentEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment exceptions by code.</summary>
public interface IAssignmentExceptionStore
{
    AssignmentException? Find(string code);
    void Save(AssignmentException item);
    bool Remove(string code);
    List<AssignmentException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentExceptionStore(Dictionary<string, AssignmentException> items, List<string> order) : IAssignmentExceptionStore
{
    public Dictionary<string, AssignmentException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentException item)
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

    public List<AssignmentException> List()
    {
        var outValue = new List<AssignmentException>();
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
public sealed class CachedAssignmentExceptionStore(MemoryAssignmentExceptionStore inner, AssignmentExceptionCache cache, int hits, int misses) : IAssignmentExceptionStore
{
    public MemoryAssignmentExceptionStore Inner { get; set; } = inner;
    public AssignmentExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentException? Find(string code)
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

    public void Save(AssignmentException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment exceptions. Empty fields match all.</summary>
public sealed record AssignmentExceptionQuery(string Title, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentException item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentException> Apply(List<AssignmentException> items)
    {
        var matched = Collections.Filter(items, (AssignmentException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment exception store.</summary>
public static class AssignmentExceptionStore
{
    public static IAssignmentExceptionStore NewMemoryAssignmentExceptionStore()
    {
        return new MemoryAssignmentExceptionStore(new Dictionary<string, AssignmentException>(), new List<string>());
    }

    public static IAssignmentExceptionStore NewCachedAssignmentExceptionStore(MemoryAssignmentExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentExceptionStore(inner, AssignmentExceptionCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AssignmentException> FindAssignmentExceptions(IAssignmentExceptionStore store, AssignmentExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
