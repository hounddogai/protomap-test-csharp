namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted policy ack profiles by code.</summary>
public interface IWeightedPolicyAckProfileStore
{
    WeightedPolicyAckProfile? Find(string code);
    void Save(WeightedPolicyAckProfile item);
    bool Remove(string code);
    List<WeightedPolicyAckProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedPolicyAckProfileStore(Dictionary<string, WeightedPolicyAckProfile> items, List<string> order) : IWeightedPolicyAckProfileStore
{
    public Dictionary<string, WeightedPolicyAckProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedPolicyAckProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedPolicyAckProfile item)
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

    public List<WeightedPolicyAckProfile> List()
    {
        var outValue = new List<WeightedPolicyAckProfile>();
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
public sealed class CachedWeightedPolicyAckProfileStore(MemoryWeightedPolicyAckProfileStore inner, WeightedPolicyAckProfileCache cache, int hits, int misses) : IWeightedPolicyAckProfileStore
{
    public MemoryWeightedPolicyAckProfileStore Inner { get; set; } = inner;
    public WeightedPolicyAckProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedPolicyAckProfile? Find(string code)
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

    public void Save(WeightedPolicyAckProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedPolicyAckProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted policy ack profiles. Empty fields match all.</summary>
public sealed record WeightedPolicyAckProfileQuery(string Country, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedPolicyAckProfile item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedPolicyAckProfile> Apply(List<WeightedPolicyAckProfile> items)
    {
        var matched = Collections.Filter(items, (WeightedPolicyAckProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted policy ack profile store.</summary>
public static class WeightedPolicyAckProfileStore
{
    public static IWeightedPolicyAckProfileStore NewMemoryWeightedPolicyAckProfileStore()
    {
        return new MemoryWeightedPolicyAckProfileStore(
            new Dictionary<string, WeightedPolicyAckProfile>(),
            new List<string>());
    }

    public static IWeightedPolicyAckProfileStore NewCachedWeightedPolicyAckProfileStore(MemoryWeightedPolicyAckProfileStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedPolicyAckProfileStore(
            inner,
            WeightedPolicyAckProfileCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<WeightedPolicyAckProfile> FindWeightedPolicyAckProfiles(IWeightedPolicyAckProfileStore store, WeightedPolicyAckProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
