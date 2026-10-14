namespace Acme.Workforce.Workplace.Visitor.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped visitor assignments by code.</summary>
public interface ICappedVisitorAssignmentStore
{
    CappedVisitorAssignment? Find(string code);
    void Save(CappedVisitorAssignment item);
    bool Remove(string code);
    List<CappedVisitorAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedVisitorAssignmentStore(Dictionary<string, CappedVisitorAssignment> items, List<string> order) : ICappedVisitorAssignmentStore
{
    public Dictionary<string, CappedVisitorAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedVisitorAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedVisitorAssignment item)
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

    public List<CappedVisitorAssignment> List()
    {
        var outValue = new List<CappedVisitorAssignment>();
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
public sealed class CachedCappedVisitorAssignmentStore(MemoryCappedVisitorAssignmentStore inner, CappedVisitorAssignmentCache cache, int hits, int misses) : ICappedVisitorAssignmentStore
{
    public MemoryCappedVisitorAssignmentStore Inner { get; set; } = inner;
    public CappedVisitorAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedVisitorAssignment? Find(string code)
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

    public void Save(CappedVisitorAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedVisitorAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped visitor assignments. Empty fields match all.</summary>
public sealed record CappedVisitorAssignmentQuery(string Title, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(CappedVisitorAssignment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedVisitorAssignment> Apply(List<CappedVisitorAssignment> items)
    {
        var matched = Collections.Filter(items, (CappedVisitorAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped visitor assignment store.</summary>
public static class CappedVisitorAssignmentStore
{
    public static ICappedVisitorAssignmentStore NewMemoryCappedVisitorAssignmentStore()
    {
        return new MemoryCappedVisitorAssignmentStore(
            new Dictionary<string, CappedVisitorAssignment>(),
            new List<string>());
    }

    public static ICappedVisitorAssignmentStore NewCachedCappedVisitorAssignmentStore(MemoryCappedVisitorAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedCappedVisitorAssignmentStore(
            inner,
            CappedVisitorAssignmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<CappedVisitorAssignment> FindCappedVisitorAssignments(ICappedVisitorAssignmentStore store, CappedVisitorAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
