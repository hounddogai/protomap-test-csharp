namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps cost model milestones by code.</summary>
public interface ICostModelMilestoneStore
{
    CostModelMilestone? Find(string code);
    void Save(CostModelMilestone item);
    bool Remove(string code);
    List<CostModelMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCostModelMilestoneStore(Dictionary<string, CostModelMilestone> items, List<string> order) : ICostModelMilestoneStore
{
    public Dictionary<string, CostModelMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CostModelMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CostModelMilestone item)
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

    public List<CostModelMilestone> List()
    {
        var outValue = new List<CostModelMilestone>();
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
public sealed class CachedCostModelMilestoneStore(MemoryCostModelMilestoneStore inner, CostModelMilestoneCache cache, int hits, int misses) : ICostModelMilestoneStore
{
    public MemoryCostModelMilestoneStore Inner { get; set; } = inner;
    public CostModelMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CostModelMilestone? Find(string code)
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

    public void Save(CostModelMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CostModelMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects cost model milestones. Empty fields match all.</summary>
public sealed record CostModelMilestoneQuery(string Segment, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(CostModelMilestone item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CostModelMilestone> Apply(List<CostModelMilestone> items)
    {
        var matched = Collections.Filter(items, (CostModelMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of cost model milestone store.</summary>
public static class CostModelMilestoneStore
{
    public static ICostModelMilestoneStore NewMemoryCostModelMilestoneStore()
    {
        return new MemoryCostModelMilestoneStore(new Dictionary<string, CostModelMilestone>(), new List<string>());
    }

    public static ICostModelMilestoneStore NewCachedCostModelMilestoneStore(MemoryCostModelMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedCostModelMilestoneStore(inner, CostModelMilestoneCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<CostModelMilestone> FindCostModelMilestones(ICostModelMilestoneStore store, CostModelMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
