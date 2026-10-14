namespace Acme.Workforce.Engagement.Recognition.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped recognition rules by code.</summary>
public interface ICappedRecognitionRuleStore
{
    CappedRecognitionRule? Find(string code);
    void Save(CappedRecognitionRule item);
    bool Remove(string code);
    List<CappedRecognitionRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedRecognitionRuleStore(Dictionary<string, CappedRecognitionRule> items, List<string> order) : ICappedRecognitionRuleStore
{
    public Dictionary<string, CappedRecognitionRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedRecognitionRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedRecognitionRule item)
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

    public List<CappedRecognitionRule> List()
    {
        var outValue = new List<CappedRecognitionRule>();
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
public sealed class CachedCappedRecognitionRuleStore(MemoryCappedRecognitionRuleStore inner, CappedRecognitionRuleCache cache, int hits, int misses) : ICappedRecognitionRuleStore
{
    public MemoryCappedRecognitionRuleStore Inner { get; set; } = inner;
    public CappedRecognitionRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedRecognitionRule? Find(string code)
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

    public void Save(CappedRecognitionRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedRecognitionRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped recognition rules. Empty fields match all.</summary>
public sealed record CappedRecognitionRuleQuery(string Category, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(CappedRecognitionRule item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<CappedRecognitionRule> Apply(List<CappedRecognitionRule> items)
    {
        var matched = Collections.Filter(items, (CappedRecognitionRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped recognition rule store.</summary>
public static class CappedRecognitionRuleStore
{
    public static ICappedRecognitionRuleStore NewMemoryCappedRecognitionRuleStore()
    {
        return new MemoryCappedRecognitionRuleStore(
            new Dictionary<string, CappedRecognitionRule>(),
            new List<string>());
    }

    public static ICappedRecognitionRuleStore NewCachedCappedRecognitionRuleStore(MemoryCappedRecognitionRuleStore inner, int ttl, IClock clock)
    {
        return new CachedCappedRecognitionRuleStore(inner, CappedRecognitionRuleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<CappedRecognitionRule> FindCappedRecognitionRules(ICappedRecognitionRuleStore store, CappedRecognitionRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
