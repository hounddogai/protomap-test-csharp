namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive training due claims by code.</summary>
public interface IAdaptiveTrainingDueClaimStore
{
    AdaptiveTrainingDueClaim? Find(string code);
    void Save(AdaptiveTrainingDueClaim item);
    bool Remove(string code);
    List<AdaptiveTrainingDueClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveTrainingDueClaimStore(Dictionary<string, AdaptiveTrainingDueClaim> items, List<string> order) : IAdaptiveTrainingDueClaimStore
{
    public Dictionary<string, AdaptiveTrainingDueClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveTrainingDueClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveTrainingDueClaim item)
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

    public List<AdaptiveTrainingDueClaim> List()
    {
        var outValue = new List<AdaptiveTrainingDueClaim>();
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
public sealed class CachedAdaptiveTrainingDueClaimStore(MemoryAdaptiveTrainingDueClaimStore inner, AdaptiveTrainingDueClaimCache cache, int hits, int misses) : IAdaptiveTrainingDueClaimStore
{
    public MemoryAdaptiveTrainingDueClaimStore Inner { get; set; } = inner;
    public AdaptiveTrainingDueClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveTrainingDueClaim? Find(string code)
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

    public void Save(AdaptiveTrainingDueClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveTrainingDueClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive training due claims. Empty fields match all.</summary>
public sealed record AdaptiveTrainingDueClaimQuery(string Title, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveTrainingDueClaim item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<AdaptiveTrainingDueClaim> Apply(List<AdaptiveTrainingDueClaim> items)
    {
        var matched = Collections.Filter(items, (AdaptiveTrainingDueClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive training due claim store.</summary>
public static class AdaptiveTrainingDueClaimStore
{
    public static IAdaptiveTrainingDueClaimStore NewMemoryAdaptiveTrainingDueClaimStore()
    {
        return new MemoryAdaptiveTrainingDueClaimStore(
            new Dictionary<string, AdaptiveTrainingDueClaim>(),
            new List<string>());
    }

    public static IAdaptiveTrainingDueClaimStore NewCachedAdaptiveTrainingDueClaimStore(MemoryAdaptiveTrainingDueClaimStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveTrainingDueClaimStore(
            inner,
            AdaptiveTrainingDueClaimCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveTrainingDueClaim> FindAdaptiveTrainingDueClaims(IAdaptiveTrainingDueClaimStore store, AdaptiveTrainingDueClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
