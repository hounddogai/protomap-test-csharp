namespace Acme.Workforce.Contractors.Vendor.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped vendor assignments by code.</summary>
public interface ICappedVendorAssignmentStore
{
    CappedVendorAssignment? Find(string code);
    void Save(CappedVendorAssignment item);
    bool Remove(string code);
    List<CappedVendorAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedVendorAssignmentStore(Dictionary<string, CappedVendorAssignment> items, List<string> order) : ICappedVendorAssignmentStore
{
    public Dictionary<string, CappedVendorAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedVendorAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedVendorAssignment item)
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

    public List<CappedVendorAssignment> List()
    {
        var outValue = new List<CappedVendorAssignment>();
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
public sealed class CachedCappedVendorAssignmentStore(MemoryCappedVendorAssignmentStore inner, CappedVendorAssignmentCache cache, int hits, int misses) : ICappedVendorAssignmentStore
{
    public MemoryCappedVendorAssignmentStore Inner { get; set; } = inner;
    public CappedVendorAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedVendorAssignment? Find(string code)
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

    public void Save(CappedVendorAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedVendorAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped vendor assignments. Empty fields match all.</summary>
public sealed record CappedVendorAssignmentQuery(string Channel, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(CappedVendorAssignment item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<CappedVendorAssignment> Apply(List<CappedVendorAssignment> items)
    {
        var matched = Collections.Filter(items, (CappedVendorAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped vendor assignment store.</summary>
public static class CappedVendorAssignmentStore
{
    public static ICappedVendorAssignmentStore NewMemoryCappedVendorAssignmentStore()
    {
        return new MemoryCappedVendorAssignmentStore(
            new Dictionary<string, CappedVendorAssignment>(),
            new List<string>());
    }

    public static ICappedVendorAssignmentStore NewCachedCappedVendorAssignmentStore(MemoryCappedVendorAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedCappedVendorAssignmentStore(
            inner,
            CappedVendorAssignmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CappedVendorAssignment> FindCappedVendorAssignments(ICappedVendorAssignmentStore store, CappedVendorAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
