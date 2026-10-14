namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive training due exceptions by code.</summary>
public interface IAdaptiveTrainingDueExceptionStore
{
    AdaptiveTrainingDueException? Find(string code);
    void Save(AdaptiveTrainingDueException item);
    bool Remove(string code);
    List<AdaptiveTrainingDueException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveTrainingDueExceptionStore(Dictionary<string, AdaptiveTrainingDueException> items, List<string> order) : IAdaptiveTrainingDueExceptionStore
{
    public Dictionary<string, AdaptiveTrainingDueException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveTrainingDueException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveTrainingDueException item)
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

    public List<AdaptiveTrainingDueException> List()
    {
        var outValue = new List<AdaptiveTrainingDueException>();
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
public sealed class CachedAdaptiveTrainingDueExceptionStore(MemoryAdaptiveTrainingDueExceptionStore inner, AdaptiveTrainingDueExceptionCache cache, int hits, int misses) : IAdaptiveTrainingDueExceptionStore
{
    public MemoryAdaptiveTrainingDueExceptionStore Inner { get; set; } = inner;
    public AdaptiveTrainingDueExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveTrainingDueException? Find(string code)
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

    public void Save(AdaptiveTrainingDueException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveTrainingDueException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive training due exceptions. Empty fields match all.</summary>
public sealed record AdaptiveTrainingDueExceptionQuery(string Name, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveTrainingDueException item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveTrainingDueException> Apply(List<AdaptiveTrainingDueException> items)
    {
        var matched = Collections.Filter(items, (AdaptiveTrainingDueException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive training due exception store.</summary>
public static class AdaptiveTrainingDueExceptionStore
{
    public static IAdaptiveTrainingDueExceptionStore NewMemoryAdaptiveTrainingDueExceptionStore()
    {
        return new MemoryAdaptiveTrainingDueExceptionStore(
            new Dictionary<string, AdaptiveTrainingDueException>(),
            new List<string>());
    }

    public static IAdaptiveTrainingDueExceptionStore NewCachedAdaptiveTrainingDueExceptionStore(MemoryAdaptiveTrainingDueExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveTrainingDueExceptionStore(
            inner,
            AdaptiveTrainingDueExceptionCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveTrainingDueException> FindAdaptiveTrainingDueExceptions(IAdaptiveTrainingDueExceptionStore store, AdaptiveTrainingDueExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
