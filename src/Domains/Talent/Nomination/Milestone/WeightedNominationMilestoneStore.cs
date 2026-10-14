namespace Acme.Workforce.Talent.Nomination.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted nomination milestones by code.</summary>
public interface IWeightedNominationMilestoneStore
{
    WeightedNominationMilestone? Find(string code);
    void Save(WeightedNominationMilestone item);
    bool Remove(string code);
    List<WeightedNominationMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedNominationMilestoneStore(Dictionary<string, WeightedNominationMilestone> items, List<string> order) : IWeightedNominationMilestoneStore
{
    public Dictionary<string, WeightedNominationMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedNominationMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedNominationMilestone item)
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

    public List<WeightedNominationMilestone> List()
    {
        var outValue = new List<WeightedNominationMilestone>();
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
public sealed class CachedWeightedNominationMilestoneStore(MemoryWeightedNominationMilestoneStore inner, WeightedNominationMilestoneCache cache, int hits, int misses) : IWeightedNominationMilestoneStore
{
    public MemoryWeightedNominationMilestoneStore Inner { get; set; } = inner;
    public WeightedNominationMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedNominationMilestone? Find(string code)
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

    public void Save(WeightedNominationMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedNominationMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted nomination milestones. Empty fields match all.</summary>
public sealed record WeightedNominationMilestoneQuery(string Category, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedNominationMilestone item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<WeightedNominationMilestone> Apply(List<WeightedNominationMilestone> items)
    {
        var matched = Collections.Filter(items, (WeightedNominationMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted nomination milestone store.</summary>
public static class WeightedNominationMilestoneStore
{
    public static IWeightedNominationMilestoneStore NewMemoryWeightedNominationMilestoneStore()
    {
        return new MemoryWeightedNominationMilestoneStore(
            new Dictionary<string, WeightedNominationMilestone>(),
            new List<string>());
    }

    public static IWeightedNominationMilestoneStore NewCachedWeightedNominationMilestoneStore(MemoryWeightedNominationMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedNominationMilestoneStore(
            inner,
            WeightedNominationMilestoneCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<WeightedNominationMilestone> FindWeightedNominationMilestones(IWeightedNominationMilestoneStore store, WeightedNominationMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
