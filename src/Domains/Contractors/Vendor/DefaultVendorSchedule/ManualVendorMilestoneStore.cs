namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual vendor milestones by code.</summary>
public interface IManualVendorMilestoneStore
{
    ManualVendorMilestone? Find(string code);
    void Save(ManualVendorMilestone item);
    bool Remove(string code);
    List<ManualVendorMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualVendorMilestoneStore(Dictionary<string, ManualVendorMilestone> items, List<string> order) : IManualVendorMilestoneStore
{
    public Dictionary<string, ManualVendorMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualVendorMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualVendorMilestone item)
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

    public List<ManualVendorMilestone> List()
    {
        var outValue = new List<ManualVendorMilestone>();
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
public sealed class CachedManualVendorMilestoneStore(MemoryManualVendorMilestoneStore inner, ManualVendorMilestoneCache cache, int hits, int misses) : IManualVendorMilestoneStore
{
    public MemoryManualVendorMilestoneStore Inner { get; set; } = inner;
    public ManualVendorMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualVendorMilestone? Find(string code)
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

    public void Save(ManualVendorMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualVendorMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual vendor milestones. Empty fields match all.</summary>
public sealed record ManualVendorMilestoneQuery(string Name, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(ManualVendorMilestone item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualVendorMilestone> Apply(List<ManualVendorMilestone> items)
    {
        var matched = Collections.Filter(items, (ManualVendorMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual vendor milestone store.</summary>
public static class ManualVendorMilestoneStore
{
    public static IManualVendorMilestoneStore NewMemoryManualVendorMilestoneStore()
    {
        return new MemoryManualVendorMilestoneStore(
            new Dictionary<string, ManualVendorMilestone>(),
            new List<string>());
    }

    public static IManualVendorMilestoneStore NewCachedManualVendorMilestoneStore(MemoryManualVendorMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedManualVendorMilestoneStore(inner, ManualVendorMilestoneCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ManualVendorMilestone> FindManualVendorMilestones(IManualVendorMilestoneStore store, ManualVendorMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
