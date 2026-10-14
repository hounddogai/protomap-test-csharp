namespace Acme.Workforce.Contractors.Offboarding.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding rules by code.</summary>
public interface IOffboardingRuleStore
{
    OffboardingRule? Find(string code);
    void Save(OffboardingRule item);
    bool Remove(string code);
    List<OffboardingRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingRuleStore(Dictionary<string, OffboardingRule> items, List<string> order) : IOffboardingRuleStore
{
    public Dictionary<string, OffboardingRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingRule item)
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

    public List<OffboardingRule> List()
    {
        var outValue = new List<OffboardingRule>();
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
public sealed class CachedOffboardingRuleStore(MemoryOffboardingRuleStore inner, OffboardingRuleCache cache, int hits, int misses) : IOffboardingRuleStore
{
    public MemoryOffboardingRuleStore Inner { get; set; } = inner;
    public OffboardingRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingRule? Find(string code)
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

    public void Save(OffboardingRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding rules. Empty fields match all.</summary>
public sealed record OffboardingRuleQuery(string Label, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingRule item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<OffboardingRule> Apply(List<OffboardingRule> items)
    {
        var matched = Collections.Filter(items, (OffboardingRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding rule store.</summary>
public static class OffboardingRuleStore
{
    public static IOffboardingRuleStore NewMemoryOffboardingRuleStore()
    {
        return new MemoryOffboardingRuleStore(new Dictionary<string, OffboardingRule>(), new List<string>());
    }

    public static IOffboardingRuleStore NewCachedOffboardingRuleStore(MemoryOffboardingRuleStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingRuleStore(inner, OffboardingRuleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<OffboardingRule> FindOffboardingRules(IOffboardingRuleStore store, OffboardingRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
