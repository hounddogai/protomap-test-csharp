namespace Acme.Workforce.Assets.Warranty.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty assignments by code.</summary>
public interface IWarrantyAssignmentStore
{
    WarrantyAssignment? Find(string code);
    void Save(WarrantyAssignment item);
    bool Remove(string code);
    List<WarrantyAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyAssignmentStore(Dictionary<string, WarrantyAssignment> items, List<string> order) : IWarrantyAssignmentStore
{
    public Dictionary<string, WarrantyAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyAssignment item)
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

    public List<WarrantyAssignment> List()
    {
        var outValue = new List<WarrantyAssignment>();
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
public sealed class CachedWarrantyAssignmentStore(MemoryWarrantyAssignmentStore inner, WarrantyAssignmentCache cache, int hits, int misses) : IWarrantyAssignmentStore
{
    public MemoryWarrantyAssignmentStore Inner { get; set; } = inner;
    public WarrantyAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyAssignment? Find(string code)
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

    public void Save(WarrantyAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty assignments. Empty fields match all.</summary>
public sealed record WarrantyAssignmentQuery(string Department, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyAssignment item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyAssignment> Apply(List<WarrantyAssignment> items)
    {
        var matched = Collections.Filter(items, (WarrantyAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty assignment store.</summary>
public static class WarrantyAssignmentStore
{
    public static IWarrantyAssignmentStore NewMemoryWarrantyAssignmentStore()
    {
        return new MemoryWarrantyAssignmentStore(new Dictionary<string, WarrantyAssignment>(), new List<string>());
    }

    public static IWarrantyAssignmentStore NewCachedWarrantyAssignmentStore(MemoryWarrantyAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyAssignmentStore(inner, WarrantyAssignmentCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<WarrantyAssignment> FindWarrantyAssignments(IWarrantyAssignmentStore store, WarrantyAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
