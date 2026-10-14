namespace Acme.Workforce.Contractors.Engagement.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual engagement rules by code.</summary>
public interface IManualEngagementRuleStore
{
    ManualEngagementRule? Find(string code);
    void Save(ManualEngagementRule item);
    bool Remove(string code);
    List<ManualEngagementRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualEngagementRuleStore(Dictionary<string, ManualEngagementRule> items, List<string> order) : IManualEngagementRuleStore
{
    public Dictionary<string, ManualEngagementRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualEngagementRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualEngagementRule item)
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

    public List<ManualEngagementRule> List()
    {
        var outValue = new List<ManualEngagementRule>();
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
public sealed class CachedManualEngagementRuleStore(MemoryManualEngagementRuleStore inner, ManualEngagementRuleCache cache, int hits, int misses) : IManualEngagementRuleStore
{
    public MemoryManualEngagementRuleStore Inner { get; set; } = inner;
    public ManualEngagementRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualEngagementRule? Find(string code)
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

    public void Save(ManualEngagementRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualEngagementRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual engagement rules. Empty fields match all.</summary>
public sealed record ManualEngagementRuleQuery(string Label, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(ManualEngagementRule item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualEngagementRule> Apply(List<ManualEngagementRule> items)
    {
        var matched = Collections.Filter(items, (ManualEngagementRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual engagement rule store.</summary>
public static class ManualEngagementRuleStore
{
    public static IManualEngagementRuleStore NewMemoryManualEngagementRuleStore()
    {
        return new MemoryManualEngagementRuleStore(
            new Dictionary<string, ManualEngagementRule>(),
            new List<string>());
    }

    public static IManualEngagementRuleStore NewCachedManualEngagementRuleStore(MemoryManualEngagementRuleStore inner, int ttl, IClock clock)
    {
        return new CachedManualEngagementRuleStore(inner, ManualEngagementRuleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ManualEngagementRule> FindManualEngagementRules(IManualEngagementRuleStore store, ManualEngagementRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
