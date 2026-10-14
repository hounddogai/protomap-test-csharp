namespace Acme.Workforce.Engagement.Recognition.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition rules by code.</summary>
public interface IRecognitionRuleStore
{
    RecognitionRule? Find(string code);
    void Save(RecognitionRule item);
    bool Remove(string code);
    List<RecognitionRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionRuleStore(Dictionary<string, RecognitionRule> items, List<string> order) : IRecognitionRuleStore
{
    public Dictionary<string, RecognitionRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionRule item)
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

    public List<RecognitionRule> List()
    {
        var outValue = new List<RecognitionRule>();
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
public sealed class CachedRecognitionRuleStore(MemoryRecognitionRuleStore inner, RecognitionRuleCache cache, int hits, int misses) : IRecognitionRuleStore
{
    public MemoryRecognitionRuleStore Inner { get; set; } = inner;
    public RecognitionRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionRule? Find(string code)
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

    public void Save(RecognitionRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition rules. Empty fields match all.</summary>
public sealed record RecognitionRuleQuery(string Channel, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionRule item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<RecognitionRule> Apply(List<RecognitionRule> items)
    {
        var matched = Collections.Filter(items, (RecognitionRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition rule store.</summary>
public static class RecognitionRuleStore
{
    public static IRecognitionRuleStore NewMemoryRecognitionRuleStore()
    {
        return new MemoryRecognitionRuleStore(new Dictionary<string, RecognitionRule>(), new List<string>());
    }

    public static IRecognitionRuleStore NewCachedRecognitionRuleStore(MemoryRecognitionRuleStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionRuleStore(inner, RecognitionRuleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RecognitionRule> FindRecognitionRules(IRecognitionRuleStore store, RecognitionRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
