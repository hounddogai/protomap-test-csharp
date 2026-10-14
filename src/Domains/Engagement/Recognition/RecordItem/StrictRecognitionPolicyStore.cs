namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict recognition policies by code.</summary>
public interface IStrictRecognitionPolicyStore
{
    StrictRecognitionPolicy? Find(string code);
    void Save(StrictRecognitionPolicy item);
    bool Remove(string code);
    List<StrictRecognitionPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictRecognitionPolicyStore(Dictionary<string, StrictRecognitionPolicy> items, List<string> order) : IStrictRecognitionPolicyStore
{
    public Dictionary<string, StrictRecognitionPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictRecognitionPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictRecognitionPolicy item)
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

    public List<StrictRecognitionPolicy> List()
    {
        var outValue = new List<StrictRecognitionPolicy>();
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
public sealed class CachedStrictRecognitionPolicyStore(MemoryStrictRecognitionPolicyStore inner, StrictRecognitionPolicyCache cache, int hits, int misses) : IStrictRecognitionPolicyStore
{
    public MemoryStrictRecognitionPolicyStore Inner { get; set; } = inner;
    public StrictRecognitionPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictRecognitionPolicy? Find(string code)
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

    public void Save(StrictRecognitionPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictRecognitionPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict recognition policies. Empty fields match all.</summary>
public sealed record StrictRecognitionPolicyQuery(string Country, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(StrictRecognitionPolicy item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictRecognitionPolicy> Apply(List<StrictRecognitionPolicy> items)
    {
        var matched = Collections.Filter(items, (StrictRecognitionPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict recognition policy store.</summary>
public static class StrictRecognitionPolicyStore
{
    public static IStrictRecognitionPolicyStore NewMemoryStrictRecognitionPolicyStore()
    {
        return new MemoryStrictRecognitionPolicyStore(
            new Dictionary<string, StrictRecognitionPolicy>(),
            new List<string>());
    }

    public static IStrictRecognitionPolicyStore NewCachedStrictRecognitionPolicyStore(MemoryStrictRecognitionPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedStrictRecognitionPolicyStore(
            inner,
            StrictRecognitionPolicyCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<StrictRecognitionPolicy> FindStrictRecognitionPolicies(IStrictRecognitionPolicyStore store, StrictRecognitionPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
