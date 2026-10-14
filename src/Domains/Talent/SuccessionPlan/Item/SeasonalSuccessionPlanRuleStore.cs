namespace Acme.Workforce.Talent.SuccessionPlan.Item;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal succession plan rules by code.</summary>
public interface ISeasonalSuccessionPlanRuleStore
{
    SeasonalSuccessionPlanRule? Find(string code);
    void Save(SeasonalSuccessionPlanRule item);
    bool Remove(string code);
    List<SeasonalSuccessionPlanRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalSuccessionPlanRuleStore(Dictionary<string, SeasonalSuccessionPlanRule> items, List<string> order) : ISeasonalSuccessionPlanRuleStore
{
    public Dictionary<string, SeasonalSuccessionPlanRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalSuccessionPlanRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalSuccessionPlanRule item)
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

    public List<SeasonalSuccessionPlanRule> List()
    {
        var outValue = new List<SeasonalSuccessionPlanRule>();
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
public sealed class CachedSeasonalSuccessionPlanRuleStore(MemorySeasonalSuccessionPlanRuleStore inner, SeasonalSuccessionPlanRuleCache cache, int hits, int misses) : ISeasonalSuccessionPlanRuleStore
{
    public MemorySeasonalSuccessionPlanRuleStore Inner { get; set; } = inner;
    public SeasonalSuccessionPlanRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalSuccessionPlanRule? Find(string code)
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

    public void Save(SeasonalSuccessionPlanRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalSuccessionPlanRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal succession plan rules. Empty fields match all.</summary>
public sealed record SeasonalSuccessionPlanRuleQuery(string Title, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalSuccessionPlanRule item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalSuccessionPlanRule> Apply(List<SeasonalSuccessionPlanRule> items)
    {
        var matched = Collections.Filter(items, (SeasonalSuccessionPlanRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal succession plan rule store.</summary>
public static class SeasonalSuccessionPlanRuleStore
{
    public static ISeasonalSuccessionPlanRuleStore NewMemorySeasonalSuccessionPlanRuleStore()
    {
        return new MemorySeasonalSuccessionPlanRuleStore(
            new Dictionary<string, SeasonalSuccessionPlanRule>(),
            new List<string>());
    }

    public static ISeasonalSuccessionPlanRuleStore NewCachedSeasonalSuccessionPlanRuleStore(MemorySeasonalSuccessionPlanRuleStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalSuccessionPlanRuleStore(
            inner,
            SeasonalSuccessionPlanRuleCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalSuccessionPlanRule> FindSeasonalSuccessionPlanRules(ISeasonalSuccessionPlanRuleStore store, SeasonalSuccessionPlanRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
