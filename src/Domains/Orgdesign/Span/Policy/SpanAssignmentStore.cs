namespace Acme.Workforce.Orgdesign.Span.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span assignments by code.</summary>
public interface ISpanAssignmentStore
{
    SpanAssignment? Find(string code);
    void Save(SpanAssignment item);
    bool Remove(string code);
    List<SpanAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanAssignmentStore(Dictionary<string, SpanAssignment> items, List<string> order) : ISpanAssignmentStore
{
    public Dictionary<string, SpanAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanAssignment item)
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

    public List<SpanAssignment> List()
    {
        var outValue = new List<SpanAssignment>();
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
public sealed class CachedSpanAssignmentStore(MemorySpanAssignmentStore inner, SpanAssignmentCache cache, int hits, int misses) : ISpanAssignmentStore
{
    public MemorySpanAssignmentStore Inner { get; set; } = inner;
    public SpanAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanAssignment? Find(string code)
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

    public void Save(SpanAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span assignments. Empty fields match all.</summary>
public sealed record SpanAssignmentQuery(string Name, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(SpanAssignment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanAssignment> Apply(List<SpanAssignment> items)
    {
        var matched = Collections.Filter(items, (SpanAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span assignment store.</summary>
public static class SpanAssignmentStore
{
    public static ISpanAssignmentStore NewMemorySpanAssignmentStore()
    {
        return new MemorySpanAssignmentStore(new Dictionary<string, SpanAssignment>(), new List<string>());
    }

    public static ISpanAssignmentStore NewCachedSpanAssignmentStore(MemorySpanAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedSpanAssignmentStore(inner, SpanAssignmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SpanAssignment> FindSpanAssignments(ISpanAssignmentStore store, SpanAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
