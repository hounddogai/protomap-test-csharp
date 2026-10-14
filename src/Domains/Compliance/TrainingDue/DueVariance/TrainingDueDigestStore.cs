namespace Acme.Workforce.Compliance.TrainingDue.DueVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due digests by code.</summary>
public interface ITrainingDueDigestStore
{
    TrainingDueDigest? Find(string code);
    void Save(TrainingDueDigest item);
    bool Remove(string code);
    List<TrainingDueDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueDigestStore(Dictionary<string, TrainingDueDigest> items, List<string> order) : ITrainingDueDigestStore
{
    public Dictionary<string, TrainingDueDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueDigest item)
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

    public List<TrainingDueDigest> List()
    {
        var outValue = new List<TrainingDueDigest>();
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
public sealed class CachedTrainingDueDigestStore(MemoryTrainingDueDigestStore inner, TrainingDueDigestCache cache, int hits, int misses) : ITrainingDueDigestStore
{
    public MemoryTrainingDueDigestStore Inner { get; set; } = inner;
    public TrainingDueDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueDigest? Find(string code)
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

    public void Save(TrainingDueDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due digests. Empty fields match all.</summary>
public sealed record TrainingDueDigestQuery(string Country, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueDigest item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<TrainingDueDigest> Apply(List<TrainingDueDigest> items)
    {
        var matched = Collections.Filter(items, (TrainingDueDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due digest store.</summary>
public static class TrainingDueDigestStore
{
    public static ITrainingDueDigestStore NewMemoryTrainingDueDigestStore()
    {
        return new MemoryTrainingDueDigestStore(new Dictionary<string, TrainingDueDigest>(), new List<string>());
    }

    public static ITrainingDueDigestStore NewCachedTrainingDueDigestStore(MemoryTrainingDueDigestStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueDigestStore(inner, TrainingDueDigestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TrainingDueDigest> FindTrainingDueDigests(ITrainingDueDigestStore store, TrainingDueDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
