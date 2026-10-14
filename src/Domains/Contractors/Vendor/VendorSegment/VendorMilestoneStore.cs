namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor milestones by code.</summary>
public interface IVendorMilestoneStore
{
    VendorMilestone? Find(string code);
    void Save(VendorMilestone item);
    bool Remove(string code);
    List<VendorMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorMilestoneStore(Dictionary<string, VendorMilestone> items, List<string> order) : IVendorMilestoneStore
{
    public Dictionary<string, VendorMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorMilestone item)
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

    public List<VendorMilestone> List()
    {
        var outValue = new List<VendorMilestone>();
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
public sealed class CachedVendorMilestoneStore(MemoryVendorMilestoneStore inner, VendorMilestoneCache cache, int hits, int misses) : IVendorMilestoneStore
{
    public MemoryVendorMilestoneStore Inner { get; set; } = inner;
    public VendorMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorMilestone? Find(string code)
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

    public void Save(VendorMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor milestones. Empty fields match all.</summary>
public sealed record VendorMilestoneQuery(string Region, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(VendorMilestone item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<VendorMilestone> Apply(List<VendorMilestone> items)
    {
        var matched = Collections.Filter(items, (VendorMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor milestone store.</summary>
public static class VendorMilestoneStore
{
    public static IVendorMilestoneStore NewMemoryVendorMilestoneStore()
    {
        return new MemoryVendorMilestoneStore(new Dictionary<string, VendorMilestone>(), new List<string>());
    }

    public static IVendorMilestoneStore NewCachedVendorMilestoneStore(MemoryVendorMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedVendorMilestoneStore(inner, VendorMilestoneCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<VendorMilestone> FindVendorMilestones(IVendorMilestoneStore store, VendorMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
