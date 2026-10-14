namespace Acme.Workforce.Engagement.Recognition.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition policies by code.</summary>
public interface IRecognitionPolicyStore
{
    RecognitionPolicy? Find(string code);
    void Save(RecognitionPolicy item);
    bool Remove(string code);
    List<RecognitionPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionPolicyStore(Dictionary<string, RecognitionPolicy> items, List<string> order) : IRecognitionPolicyStore
{
    public Dictionary<string, RecognitionPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionPolicy item)
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

    public List<RecognitionPolicy> List()
    {
        var outValue = new List<RecognitionPolicy>();
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
public sealed class CachedRecognitionPolicyStore(MemoryRecognitionPolicyStore inner, RecognitionPolicyCache cache, int hits, int misses) : IRecognitionPolicyStore
{
    public MemoryRecognitionPolicyStore Inner { get; set; } = inner;
    public RecognitionPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionPolicy? Find(string code)
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

    public void Save(RecognitionPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition policies. Empty fields match all.</summary>
public sealed record RecognitionPolicyQuery(string Title, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionPolicy item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<RecognitionPolicy> Apply(List<RecognitionPolicy> items)
    {
        var matched = Collections.Filter(items, (RecognitionPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition policy store.</summary>
public static class RecognitionPolicyStore
{
    public static IRecognitionPolicyStore NewMemoryRecognitionPolicyStore()
    {
        return new MemoryRecognitionPolicyStore(new Dictionary<string, RecognitionPolicy>(), new List<string>());
    }

    public static IRecognitionPolicyStore NewCachedRecognitionPolicyStore(MemoryRecognitionPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionPolicyStore(inner, RecognitionPolicyCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RecognitionPolicy> FindRecognitionPolicies(IRecognitionPolicyStore store, RecognitionPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
