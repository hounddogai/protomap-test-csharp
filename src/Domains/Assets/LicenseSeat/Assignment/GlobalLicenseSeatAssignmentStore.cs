namespace Acme.Workforce.Assets.LicenseSeat.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global license seat assignments by code.</summary>
public interface IGlobalLicenseSeatAssignmentStore
{
    GlobalLicenseSeatAssignment? Find(string code);
    void Save(GlobalLicenseSeatAssignment item);
    bool Remove(string code);
    List<GlobalLicenseSeatAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalLicenseSeatAssignmentStore(Dictionary<string, GlobalLicenseSeatAssignment> items, List<string> order) : IGlobalLicenseSeatAssignmentStore
{
    public Dictionary<string, GlobalLicenseSeatAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalLicenseSeatAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalLicenseSeatAssignment item)
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

    public List<GlobalLicenseSeatAssignment> List()
    {
        var outValue = new List<GlobalLicenseSeatAssignment>();
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
public sealed class CachedGlobalLicenseSeatAssignmentStore(MemoryGlobalLicenseSeatAssignmentStore inner, GlobalLicenseSeatAssignmentCache cache, int hits, int misses) : IGlobalLicenseSeatAssignmentStore
{
    public MemoryGlobalLicenseSeatAssignmentStore Inner { get; set; } = inner;
    public GlobalLicenseSeatAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalLicenseSeatAssignment? Find(string code)
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

    public void Save(GlobalLicenseSeatAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalLicenseSeatAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global license seat assignments. Empty fields match all.</summary>
public sealed record GlobalLicenseSeatAssignmentQuery(string Department, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalLicenseSeatAssignment item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<GlobalLicenseSeatAssignment> Apply(List<GlobalLicenseSeatAssignment> items)
    {
        var matched = Collections.Filter(items, (GlobalLicenseSeatAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global license seat assignment store.</summary>
public static class GlobalLicenseSeatAssignmentStore
{
    public static IGlobalLicenseSeatAssignmentStore NewMemoryGlobalLicenseSeatAssignmentStore()
    {
        return new MemoryGlobalLicenseSeatAssignmentStore(
            new Dictionary<string, GlobalLicenseSeatAssignment>(),
            new List<string>());
    }

    public static IGlobalLicenseSeatAssignmentStore NewCachedGlobalLicenseSeatAssignmentStore(MemoryGlobalLicenseSeatAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalLicenseSeatAssignmentStore(
            inner,
            GlobalLicenseSeatAssignmentCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<GlobalLicenseSeatAssignment> FindGlobalLicenseSeatAssignments(IGlobalLicenseSeatAssignmentStore store, GlobalLicenseSeatAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
