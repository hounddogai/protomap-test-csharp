namespace Acme.Workforce.Talent.CareerPath.PathRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global career path rules by code.</summary>
public interface IGlobalCareerPathRuleStore
{
    GlobalCareerPathRule? Find(string code);
    void Save(GlobalCareerPathRule item);
    bool Remove(string code);
    List<GlobalCareerPathRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalCareerPathRuleStore(Dictionary<string, GlobalCareerPathRule> items, List<string> order) : IGlobalCareerPathRuleStore
{
    public Dictionary<string, GlobalCareerPathRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalCareerPathRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalCareerPathRule item)
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

    public List<GlobalCareerPathRule> List()
    {
        var outValue = new List<GlobalCareerPathRule>();
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
public sealed class CachedGlobalCareerPathRuleStore(MemoryGlobalCareerPathRuleStore inner, GlobalCareerPathRuleCache cache, int hits, int misses) : IGlobalCareerPathRuleStore
{
    public MemoryGlobalCareerPathRuleStore Inner { get; set; } = inner;
    public GlobalCareerPathRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalCareerPathRule? Find(string code)
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

    public void Save(GlobalCareerPathRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalCareerPathRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global career path rules. Empty fields match all.</summary>
public sealed record GlobalCareerPathRuleQuery(string Segment, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalCareerPathRule item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalCareerPathRule> Apply(List<GlobalCareerPathRule> items)
    {
        var matched = Collections.Filter(items, (GlobalCareerPathRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global career path rule store.</summary>
public static class GlobalCareerPathRuleStore
{
    public static IGlobalCareerPathRuleStore NewMemoryGlobalCareerPathRuleStore()
    {
        return new MemoryGlobalCareerPathRuleStore(
            new Dictionary<string, GlobalCareerPathRule>(),
            new List<string>());
    }

    public static IGlobalCareerPathRuleStore NewCachedGlobalCareerPathRuleStore(MemoryGlobalCareerPathRuleStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalCareerPathRuleStore(inner, GlobalCareerPathRuleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<GlobalCareerPathRule> FindGlobalCareerPathRules(IGlobalCareerPathRuleStore store, GlobalCareerPathRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
