namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted reorg claims by code.</summary>
public interface IWeightedReorgClaimStore
{
    WeightedReorgClaim? Find(string code);
    void Save(WeightedReorgClaim item);
    bool Remove(string code);
    List<WeightedReorgClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedReorgClaimStore(Dictionary<string, WeightedReorgClaim> items, List<string> order) : IWeightedReorgClaimStore
{
    public Dictionary<string, WeightedReorgClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedReorgClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedReorgClaim item)
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

    public List<WeightedReorgClaim> List()
    {
        var outValue = new List<WeightedReorgClaim>();
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
public sealed class CachedWeightedReorgClaimStore(MemoryWeightedReorgClaimStore inner, WeightedReorgClaimCache cache, int hits, int misses) : IWeightedReorgClaimStore
{
    public MemoryWeightedReorgClaimStore Inner { get; set; } = inner;
    public WeightedReorgClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedReorgClaim? Find(string code)
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

    public void Save(WeightedReorgClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedReorgClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted reorg claims. Empty fields match all.</summary>
public sealed record WeightedReorgClaimQuery(string Name, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedReorgClaim item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedReorgClaim> Apply(List<WeightedReorgClaim> items)
    {
        var matched = Collections.Filter(items, (WeightedReorgClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted reorg claim store.</summary>
public static class WeightedReorgClaimStore
{
    public static IWeightedReorgClaimStore NewMemoryWeightedReorgClaimStore()
    {
        return new MemoryWeightedReorgClaimStore(new Dictionary<string, WeightedReorgClaim>(), new List<string>());
    }

    public static IWeightedReorgClaimStore NewCachedWeightedReorgClaimStore(MemoryWeightedReorgClaimStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedReorgClaimStore(inner, WeightedReorgClaimCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<WeightedReorgClaim> FindWeightedReorgClaims(IWeightedReorgClaimStore store, WeightedReorgClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
