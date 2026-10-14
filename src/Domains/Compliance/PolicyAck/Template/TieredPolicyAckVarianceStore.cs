namespace Acme.Workforce.Compliance.PolicyAck.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered policy ack variances by code.</summary>
public interface ITieredPolicyAckVarianceStore
{
    TieredPolicyAckVariance? Find(string code);
    void Save(TieredPolicyAckVariance item);
    bool Remove(string code);
    List<TieredPolicyAckVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredPolicyAckVarianceStore(Dictionary<string, TieredPolicyAckVariance> items, List<string> order) : ITieredPolicyAckVarianceStore
{
    public Dictionary<string, TieredPolicyAckVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredPolicyAckVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredPolicyAckVariance item)
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

    public List<TieredPolicyAckVariance> List()
    {
        var outValue = new List<TieredPolicyAckVariance>();
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
public sealed class CachedTieredPolicyAckVarianceStore(MemoryTieredPolicyAckVarianceStore inner, TieredPolicyAckVarianceCache cache, int hits, int misses) : ITieredPolicyAckVarianceStore
{
    public MemoryTieredPolicyAckVarianceStore Inner { get; set; } = inner;
    public TieredPolicyAckVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredPolicyAckVariance? Find(string code)
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

    public void Save(TieredPolicyAckVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredPolicyAckVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered policy ack variances. Empty fields match all.</summary>
public sealed record TieredPolicyAckVarianceQuery(string Source, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(TieredPolicyAckVariance item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredPolicyAckVariance> Apply(List<TieredPolicyAckVariance> items)
    {
        var matched = Collections.Filter(items, (TieredPolicyAckVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered policy ack variance store.</summary>
public static class TieredPolicyAckVarianceStore
{
    public static ITieredPolicyAckVarianceStore NewMemoryTieredPolicyAckVarianceStore()
    {
        return new MemoryTieredPolicyAckVarianceStore(
            new Dictionary<string, TieredPolicyAckVariance>(),
            new List<string>());
    }

    public static ITieredPolicyAckVarianceStore NewCachedTieredPolicyAckVarianceStore(MemoryTieredPolicyAckVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedTieredPolicyAckVarianceStore(
            inner,
            TieredPolicyAckVarianceCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<TieredPolicyAckVariance> FindTieredPolicyAckVariances(ITieredPolicyAckVarianceStore store, TieredPolicyAckVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
