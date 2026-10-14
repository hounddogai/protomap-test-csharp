namespace Acme.Workforce.Payrollops.PayRun.RunVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered pay run milestones by code.</summary>
public interface ITieredPayRunMilestoneStore
{
    TieredPayRunMilestone? Find(string code);
    void Save(TieredPayRunMilestone item);
    bool Remove(string code);
    List<TieredPayRunMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredPayRunMilestoneStore(Dictionary<string, TieredPayRunMilestone> items, List<string> order) : ITieredPayRunMilestoneStore
{
    public Dictionary<string, TieredPayRunMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredPayRunMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredPayRunMilestone item)
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

    public List<TieredPayRunMilestone> List()
    {
        var outValue = new List<TieredPayRunMilestone>();
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
public sealed class CachedTieredPayRunMilestoneStore(MemoryTieredPayRunMilestoneStore inner, TieredPayRunMilestoneCache cache, int hits, int misses) : ITieredPayRunMilestoneStore
{
    public MemoryTieredPayRunMilestoneStore Inner { get; set; } = inner;
    public TieredPayRunMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredPayRunMilestone? Find(string code)
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

    public void Save(TieredPayRunMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredPayRunMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered pay run milestones. Empty fields match all.</summary>
public sealed record TieredPayRunMilestoneQuery(string Label, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(TieredPayRunMilestone item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<TieredPayRunMilestone> Apply(List<TieredPayRunMilestone> items)
    {
        var matched = Collections.Filter(items, (TieredPayRunMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered pay run milestone store.</summary>
public static class TieredPayRunMilestoneStore
{
    public static ITieredPayRunMilestoneStore NewMemoryTieredPayRunMilestoneStore()
    {
        return new MemoryTieredPayRunMilestoneStore(
            new Dictionary<string, TieredPayRunMilestone>(),
            new List<string>());
    }

    public static ITieredPayRunMilestoneStore NewCachedTieredPayRunMilestoneStore(MemoryTieredPayRunMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedTieredPayRunMilestoneStore(inner, TieredPayRunMilestoneCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TieredPayRunMilestone> FindTieredPayRunMilestones(ITieredPayRunMilestoneStore store, TieredPayRunMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
