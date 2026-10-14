namespace Acme.Workforce.Compliance.PolicyAck.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted policy ack bundles by code.</summary>
public interface IWeightedPolicyAckBundleStore
{
    WeightedPolicyAckBundle? Find(string code);
    void Save(WeightedPolicyAckBundle item);
    bool Remove(string code);
    List<WeightedPolicyAckBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedPolicyAckBundleStore(Dictionary<string, WeightedPolicyAckBundle> items, List<string> order) : IWeightedPolicyAckBundleStore
{
    public Dictionary<string, WeightedPolicyAckBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedPolicyAckBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedPolicyAckBundle item)
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

    public List<WeightedPolicyAckBundle> List()
    {
        var outValue = new List<WeightedPolicyAckBundle>();
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
public sealed class CachedWeightedPolicyAckBundleStore(MemoryWeightedPolicyAckBundleStore inner, WeightedPolicyAckBundleCache cache, int hits, int misses) : IWeightedPolicyAckBundleStore
{
    public MemoryWeightedPolicyAckBundleStore Inner { get; set; } = inner;
    public WeightedPolicyAckBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedPolicyAckBundle? Find(string code)
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

    public void Save(WeightedPolicyAckBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedPolicyAckBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted policy ack bundles. Empty fields match all.</summary>
public sealed record WeightedPolicyAckBundleQuery(string Label, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedPolicyAckBundle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedPolicyAckBundle> Apply(List<WeightedPolicyAckBundle> items)
    {
        var matched = Collections.Filter(items, (WeightedPolicyAckBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted policy ack bundle store.</summary>
public static class WeightedPolicyAckBundleStore
{
    public static IWeightedPolicyAckBundleStore NewMemoryWeightedPolicyAckBundleStore()
    {
        return new MemoryWeightedPolicyAckBundleStore(
            new Dictionary<string, WeightedPolicyAckBundle>(),
            new List<string>());
    }

    public static IWeightedPolicyAckBundleStore NewCachedWeightedPolicyAckBundleStore(MemoryWeightedPolicyAckBundleStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedPolicyAckBundleStore(
            inner,
            WeightedPolicyAckBundleCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<WeightedPolicyAckBundle> FindWeightedPolicyAckBundles(IWeightedPolicyAckBundleStore store, WeightedPolicyAckBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
