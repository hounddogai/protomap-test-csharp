namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional license seat milestones by code.</summary>
public interface IRegionalLicenseSeatMilestoneStore
{
    RegionalLicenseSeatMilestone? Find(string code);
    void Save(RegionalLicenseSeatMilestone item);
    bool Remove(string code);
    List<RegionalLicenseSeatMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalLicenseSeatMilestoneStore(Dictionary<string, RegionalLicenseSeatMilestone> items, List<string> order) : IRegionalLicenseSeatMilestoneStore
{
    public Dictionary<string, RegionalLicenseSeatMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalLicenseSeatMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalLicenseSeatMilestone item)
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

    public List<RegionalLicenseSeatMilestone> List()
    {
        var outValue = new List<RegionalLicenseSeatMilestone>();
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
public sealed class CachedRegionalLicenseSeatMilestoneStore(MemoryRegionalLicenseSeatMilestoneStore inner, RegionalLicenseSeatMilestoneCache cache, int hits, int misses) : IRegionalLicenseSeatMilestoneStore
{
    public MemoryRegionalLicenseSeatMilestoneStore Inner { get; set; } = inner;
    public RegionalLicenseSeatMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalLicenseSeatMilestone? Find(string code)
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

    public void Save(RegionalLicenseSeatMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalLicenseSeatMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional license seat milestones. Empty fields match all.</summary>
public sealed record RegionalLicenseSeatMilestoneQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalLicenseSeatMilestone item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalLicenseSeatMilestone> Apply(List<RegionalLicenseSeatMilestone> items)
    {
        var matched = Collections.Filter(items, (RegionalLicenseSeatMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional license seat milestone store.</summary>
public static class RegionalLicenseSeatMilestoneStore
{
    public static IRegionalLicenseSeatMilestoneStore NewMemoryRegionalLicenseSeatMilestoneStore()
    {
        return new MemoryRegionalLicenseSeatMilestoneStore(
            new Dictionary<string, RegionalLicenseSeatMilestone>(),
            new List<string>());
    }

    public static IRegionalLicenseSeatMilestoneStore NewCachedRegionalLicenseSeatMilestoneStore(MemoryRegionalLicenseSeatMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalLicenseSeatMilestoneStore(
            inner,
            RegionalLicenseSeatMilestoneCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RegionalLicenseSeatMilestone> FindRegionalLicenseSeatMilestones(IRegionalLicenseSeatMilestoneStore store, RegionalLicenseSeatMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
