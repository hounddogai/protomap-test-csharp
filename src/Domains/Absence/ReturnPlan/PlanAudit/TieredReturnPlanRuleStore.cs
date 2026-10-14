namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered return plan rules by code.</summary>
public interface ITieredReturnPlanRuleStore
{
    TieredReturnPlanRule? Find(string code);
    void Save(TieredReturnPlanRule item);
    bool Remove(string code);
    List<TieredReturnPlanRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredReturnPlanRuleStore(Dictionary<string, TieredReturnPlanRule> items, List<string> order) : ITieredReturnPlanRuleStore
{
    public Dictionary<string, TieredReturnPlanRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredReturnPlanRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredReturnPlanRule item)
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

    public List<TieredReturnPlanRule> List()
    {
        var outValue = new List<TieredReturnPlanRule>();
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
public sealed class CachedTieredReturnPlanRuleStore(MemoryTieredReturnPlanRuleStore inner, TieredReturnPlanRuleCache cache, int hits, int misses) : ITieredReturnPlanRuleStore
{
    public MemoryTieredReturnPlanRuleStore Inner { get; set; } = inner;
    public TieredReturnPlanRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredReturnPlanRule? Find(string code)
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

    public void Save(TieredReturnPlanRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredReturnPlanRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered return plan rules. Empty fields match all.</summary>
public sealed record TieredReturnPlanRuleQuery(string Team, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(TieredReturnPlanRule item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredReturnPlanRule> Apply(List<TieredReturnPlanRule> items)
    {
        var matched = Collections.Filter(items, (TieredReturnPlanRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered return plan rule store.</summary>
public static class TieredReturnPlanRuleStore
{
    public static ITieredReturnPlanRuleStore NewMemoryTieredReturnPlanRuleStore()
    {
        return new MemoryTieredReturnPlanRuleStore(
            new Dictionary<string, TieredReturnPlanRule>(),
            new List<string>());
    }

    public static ITieredReturnPlanRuleStore NewCachedTieredReturnPlanRuleStore(MemoryTieredReturnPlanRuleStore inner, int ttl, IClock clock)
    {
        return new CachedTieredReturnPlanRuleStore(inner, TieredReturnPlanRuleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TieredReturnPlanRule> FindTieredReturnPlanRules(ITieredReturnPlanRuleStore store, TieredReturnPlanRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
