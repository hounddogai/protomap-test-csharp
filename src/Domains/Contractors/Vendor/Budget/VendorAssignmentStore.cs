namespace Acme.Workforce.Contractors.Vendor.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor assignments by code.</summary>
public interface IVendorAssignmentStore
{
    VendorAssignment? Find(string code);
    void Save(VendorAssignment item);
    bool Remove(string code);
    List<VendorAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorAssignmentStore(Dictionary<string, VendorAssignment> items, List<string> order) : IVendorAssignmentStore
{
    public Dictionary<string, VendorAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorAssignment item)
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

    public List<VendorAssignment> List()
    {
        var outValue = new List<VendorAssignment>();
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
public sealed class CachedVendorAssignmentStore(MemoryVendorAssignmentStore inner, VendorAssignmentCache cache, int hits, int misses) : IVendorAssignmentStore
{
    public MemoryVendorAssignmentStore Inner { get; set; } = inner;
    public VendorAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorAssignment? Find(string code)
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

    public void Save(VendorAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor assignments. Empty fields match all.</summary>
public sealed record VendorAssignmentQuery(string Title, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(VendorAssignment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VendorAssignment> Apply(List<VendorAssignment> items)
    {
        var matched = Collections.Filter(items, (VendorAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor assignment store.</summary>
public static class VendorAssignmentStore
{
    public static IVendorAssignmentStore NewMemoryVendorAssignmentStore()
    {
        return new MemoryVendorAssignmentStore(new Dictionary<string, VendorAssignment>(), new List<string>());
    }

    public static IVendorAssignmentStore NewCachedVendorAssignmentStore(MemoryVendorAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedVendorAssignmentStore(inner, VendorAssignmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<VendorAssignment> FindVendorAssignments(IVendorAssignmentStore store, VendorAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
