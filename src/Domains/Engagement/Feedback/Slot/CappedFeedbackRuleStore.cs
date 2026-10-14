namespace Acme.Workforce.Engagement.Feedback.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped feedback rules by code.</summary>
public interface ICappedFeedbackRuleStore
{
    CappedFeedbackRule? Find(string code);
    void Save(CappedFeedbackRule item);
    bool Remove(string code);
    List<CappedFeedbackRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedFeedbackRuleStore(Dictionary<string, CappedFeedbackRule> items, List<string> order) : ICappedFeedbackRuleStore
{
    public Dictionary<string, CappedFeedbackRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedFeedbackRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedFeedbackRule item)
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

    public List<CappedFeedbackRule> List()
    {
        var outValue = new List<CappedFeedbackRule>();
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
public sealed class CachedCappedFeedbackRuleStore(MemoryCappedFeedbackRuleStore inner, CappedFeedbackRuleCache cache, int hits, int misses) : ICappedFeedbackRuleStore
{
    public MemoryCappedFeedbackRuleStore Inner { get; set; } = inner;
    public CappedFeedbackRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedFeedbackRule? Find(string code)
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

    public void Save(CappedFeedbackRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedFeedbackRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped feedback rules. Empty fields match all.</summary>
public sealed record CappedFeedbackRuleQuery(string Team, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(CappedFeedbackRule item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedFeedbackRule> Apply(List<CappedFeedbackRule> items)
    {
        var matched = Collections.Filter(items, (CappedFeedbackRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped feedback rule store.</summary>
public static class CappedFeedbackRuleStore
{
    public static ICappedFeedbackRuleStore NewMemoryCappedFeedbackRuleStore()
    {
        return new MemoryCappedFeedbackRuleStore(new Dictionary<string, CappedFeedbackRule>(), new List<string>());
    }

    public static ICappedFeedbackRuleStore NewCachedCappedFeedbackRuleStore(MemoryCappedFeedbackRuleStore inner, int ttl, IClock clock)
    {
        return new CachedCappedFeedbackRuleStore(inner, CappedFeedbackRuleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CappedFeedbackRule> FindCappedFeedbackRules(ICappedFeedbackRuleStore store, CappedFeedbackRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
