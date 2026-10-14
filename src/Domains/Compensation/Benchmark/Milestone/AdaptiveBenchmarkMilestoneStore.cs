namespace Acme.Workforce.Compensation.Benchmark.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive benchmark milestones by code.</summary>
public interface IAdaptiveBenchmarkMilestoneStore
{
    AdaptiveBenchmarkMilestone? Find(string code);
    void Save(AdaptiveBenchmarkMilestone item);
    bool Remove(string code);
    List<AdaptiveBenchmarkMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveBenchmarkMilestoneStore(Dictionary<string, AdaptiveBenchmarkMilestone> items, List<string> order) : IAdaptiveBenchmarkMilestoneStore
{
    public Dictionary<string, AdaptiveBenchmarkMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveBenchmarkMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveBenchmarkMilestone item)
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

    public List<AdaptiveBenchmarkMilestone> List()
    {
        var outValue = new List<AdaptiveBenchmarkMilestone>();
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
public sealed class CachedAdaptiveBenchmarkMilestoneStore(MemoryAdaptiveBenchmarkMilestoneStore inner, AdaptiveBenchmarkMilestoneCache cache, int hits, int misses) : IAdaptiveBenchmarkMilestoneStore
{
    public MemoryAdaptiveBenchmarkMilestoneStore Inner { get; set; } = inner;
    public AdaptiveBenchmarkMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveBenchmarkMilestone? Find(string code)
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

    public void Save(AdaptiveBenchmarkMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveBenchmarkMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive benchmark milestones. Empty fields match all.</summary>
public sealed record AdaptiveBenchmarkMilestoneQuery(string Name, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveBenchmarkMilestone item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<AdaptiveBenchmarkMilestone> Apply(List<AdaptiveBenchmarkMilestone> items)
    {
        var matched = Collections.Filter(items, (AdaptiveBenchmarkMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive benchmark milestone store.</summary>
public static class AdaptiveBenchmarkMilestoneStore
{
    public static IAdaptiveBenchmarkMilestoneStore NewMemoryAdaptiveBenchmarkMilestoneStore()
    {
        return new MemoryAdaptiveBenchmarkMilestoneStore(
            new Dictionary<string, AdaptiveBenchmarkMilestone>(),
            new List<string>());
    }

    public static IAdaptiveBenchmarkMilestoneStore NewCachedAdaptiveBenchmarkMilestoneStore(MemoryAdaptiveBenchmarkMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveBenchmarkMilestoneStore(
            inner,
            AdaptiveBenchmarkMilestoneCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveBenchmarkMilestone> FindAdaptiveBenchmarkMilestones(IAdaptiveBenchmarkMilestoneStore store, AdaptiveBenchmarkMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
