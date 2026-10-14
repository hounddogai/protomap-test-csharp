namespace Acme.Workforce.Workplace.Visitor.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default visitor assignments by code.</summary>
public interface IDefaultVisitorAssignmentStore
{
    DefaultVisitorAssignment? Find(string code);
    void Save(DefaultVisitorAssignment item);
    bool Remove(string code);
    List<DefaultVisitorAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultVisitorAssignmentStore(Dictionary<string, DefaultVisitorAssignment> items, List<string> order) : IDefaultVisitorAssignmentStore
{
    public Dictionary<string, DefaultVisitorAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultVisitorAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultVisitorAssignment item)
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

    public List<DefaultVisitorAssignment> List()
    {
        var outValue = new List<DefaultVisitorAssignment>();
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
public sealed class CachedDefaultVisitorAssignmentStore(MemoryDefaultVisitorAssignmentStore inner, DefaultVisitorAssignmentCache cache, int hits, int misses) : IDefaultVisitorAssignmentStore
{
    public MemoryDefaultVisitorAssignmentStore Inner { get; set; } = inner;
    public DefaultVisitorAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultVisitorAssignment? Find(string code)
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

    public void Save(DefaultVisitorAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultVisitorAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default visitor assignments. Empty fields match all.</summary>
public sealed record DefaultVisitorAssignmentQuery(string Source, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultVisitorAssignment item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultVisitorAssignment> Apply(List<DefaultVisitorAssignment> items)
    {
        var matched = Collections.Filter(items, (DefaultVisitorAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default visitor assignment store.</summary>
public static class DefaultVisitorAssignmentStore
{
    public static IDefaultVisitorAssignmentStore NewMemoryDefaultVisitorAssignmentStore()
    {
        return new MemoryDefaultVisitorAssignmentStore(
            new Dictionary<string, DefaultVisitorAssignment>(),
            new List<string>());
    }

    public static IDefaultVisitorAssignmentStore NewCachedDefaultVisitorAssignmentStore(MemoryDefaultVisitorAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultVisitorAssignmentStore(
            inner,
            DefaultVisitorAssignmentCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<DefaultVisitorAssignment> FindDefaultVisitorAssignments(IDefaultVisitorAssignmentStore store, DefaultVisitorAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
