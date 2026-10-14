namespace Acme.Workforce.Engagement.Feedback.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered feedback policies by code.</summary>
public interface ITieredFeedbackPolicyStore
{
    TieredFeedbackPolicy? Find(string code);
    void Save(TieredFeedbackPolicy item);
    bool Remove(string code);
    List<TieredFeedbackPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredFeedbackPolicyStore(Dictionary<string, TieredFeedbackPolicy> items, List<string> order) : ITieredFeedbackPolicyStore
{
    public Dictionary<string, TieredFeedbackPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredFeedbackPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredFeedbackPolicy item)
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

    public List<TieredFeedbackPolicy> List()
    {
        var outValue = new List<TieredFeedbackPolicy>();
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
public sealed class CachedTieredFeedbackPolicyStore(MemoryTieredFeedbackPolicyStore inner, TieredFeedbackPolicyCache cache, int hits, int misses) : ITieredFeedbackPolicyStore
{
    public MemoryTieredFeedbackPolicyStore Inner { get; set; } = inner;
    public TieredFeedbackPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredFeedbackPolicy? Find(string code)
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

    public void Save(TieredFeedbackPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredFeedbackPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered feedback policies. Empty fields match all.</summary>
public sealed record TieredFeedbackPolicyQuery(string Region, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(TieredFeedbackPolicy item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredFeedbackPolicy> Apply(List<TieredFeedbackPolicy> items)
    {
        var matched = Collections.Filter(items, (TieredFeedbackPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered feedback policy store.</summary>
public static class TieredFeedbackPolicyStore
{
    public static ITieredFeedbackPolicyStore NewMemoryTieredFeedbackPolicyStore()
    {
        return new MemoryTieredFeedbackPolicyStore(
            new Dictionary<string, TieredFeedbackPolicy>(),
            new List<string>());
    }

    public static ITieredFeedbackPolicyStore NewCachedTieredFeedbackPolicyStore(MemoryTieredFeedbackPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedTieredFeedbackPolicyStore(inner, TieredFeedbackPolicyCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TieredFeedbackPolicy> FindTieredFeedbackPolicies(ITieredFeedbackPolicyStore store, TieredFeedbackPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
