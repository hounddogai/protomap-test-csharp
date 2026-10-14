namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal cost model milestones by code.</summary>
public interface ISeasonalCostModelMilestoneStore
{
    SeasonalCostModelMilestone? Find(string code);
    void Save(SeasonalCostModelMilestone item);
    bool Remove(string code);
    List<SeasonalCostModelMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalCostModelMilestoneStore(Dictionary<string, SeasonalCostModelMilestone> items, List<string> order) : ISeasonalCostModelMilestoneStore
{
    public Dictionary<string, SeasonalCostModelMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalCostModelMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalCostModelMilestone item)
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

    public List<SeasonalCostModelMilestone> List()
    {
        var outValue = new List<SeasonalCostModelMilestone>();
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
public sealed class CachedSeasonalCostModelMilestoneStore(MemorySeasonalCostModelMilestoneStore inner, SeasonalCostModelMilestoneCache cache, int hits, int misses) : ISeasonalCostModelMilestoneStore
{
    public MemorySeasonalCostModelMilestoneStore Inner { get; set; } = inner;
    public SeasonalCostModelMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalCostModelMilestone? Find(string code)
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

    public void Save(SeasonalCostModelMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalCostModelMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal cost model milestones. Empty fields match all.</summary>
public sealed record SeasonalCostModelMilestoneQuery(string Label, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalCostModelMilestone item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalCostModelMilestone> Apply(List<SeasonalCostModelMilestone> items)
    {
        var matched = Collections.Filter(items, (SeasonalCostModelMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal cost model milestone store.</summary>
public static class SeasonalCostModelMilestoneStore
{
    public static ISeasonalCostModelMilestoneStore NewMemorySeasonalCostModelMilestoneStore()
    {
        return new MemorySeasonalCostModelMilestoneStore(
            new Dictionary<string, SeasonalCostModelMilestone>(),
            new List<string>());
    }

    public static ISeasonalCostModelMilestoneStore NewCachedSeasonalCostModelMilestoneStore(MemorySeasonalCostModelMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalCostModelMilestoneStore(
            inner,
            SeasonalCostModelMilestoneCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalCostModelMilestone> FindSeasonalCostModelMilestones(ISeasonalCostModelMilestoneStore store, SeasonalCostModelMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
