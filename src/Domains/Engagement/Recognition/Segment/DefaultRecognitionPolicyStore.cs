namespace Acme.Workforce.Engagement.Recognition.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default recognition policies by code.</summary>
public interface IDefaultRecognitionPolicyStore
{
    DefaultRecognitionPolicy? Find(string code);
    void Save(DefaultRecognitionPolicy item);
    bool Remove(string code);
    List<DefaultRecognitionPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultRecognitionPolicyStore(Dictionary<string, DefaultRecognitionPolicy> items, List<string> order) : IDefaultRecognitionPolicyStore
{
    public Dictionary<string, DefaultRecognitionPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultRecognitionPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultRecognitionPolicy item)
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

    public List<DefaultRecognitionPolicy> List()
    {
        var outValue = new List<DefaultRecognitionPolicy>();
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
public sealed class CachedDefaultRecognitionPolicyStore(MemoryDefaultRecognitionPolicyStore inner, DefaultRecognitionPolicyCache cache, int hits, int misses) : IDefaultRecognitionPolicyStore
{
    public MemoryDefaultRecognitionPolicyStore Inner { get; set; } = inner;
    public DefaultRecognitionPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultRecognitionPolicy? Find(string code)
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

    public void Save(DefaultRecognitionPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultRecognitionPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default recognition policies. Empty fields match all.</summary>
public sealed record DefaultRecognitionPolicyQuery(string Segment, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultRecognitionPolicy item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultRecognitionPolicy> Apply(List<DefaultRecognitionPolicy> items)
    {
        var matched = Collections.Filter(items, (DefaultRecognitionPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default recognition policy store.</summary>
public static class DefaultRecognitionPolicyStore
{
    public static IDefaultRecognitionPolicyStore NewMemoryDefaultRecognitionPolicyStore()
    {
        return new MemoryDefaultRecognitionPolicyStore(
            new Dictionary<string, DefaultRecognitionPolicy>(),
            new List<string>());
    }

    public static IDefaultRecognitionPolicyStore NewCachedDefaultRecognitionPolicyStore(MemoryDefaultRecognitionPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultRecognitionPolicyStore(
            inner,
            DefaultRecognitionPolicyCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<DefaultRecognitionPolicy> FindDefaultRecognitionPolicies(IDefaultRecognitionPolicyStore store, DefaultRecognitionPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
