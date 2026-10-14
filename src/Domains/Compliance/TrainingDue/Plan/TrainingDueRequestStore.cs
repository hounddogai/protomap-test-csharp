namespace Acme.Workforce.Compliance.TrainingDue.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due requests by code.</summary>
public interface ITrainingDueRequestStore
{
    TrainingDueRequest? Find(string code);
    void Save(TrainingDueRequest item);
    bool Remove(string code);
    List<TrainingDueRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueRequestStore(Dictionary<string, TrainingDueRequest> items, List<string> order) : ITrainingDueRequestStore
{
    public Dictionary<string, TrainingDueRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueRequest item)
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

    public List<TrainingDueRequest> List()
    {
        var outValue = new List<TrainingDueRequest>();
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
public sealed class CachedTrainingDueRequestStore(MemoryTrainingDueRequestStore inner, TrainingDueRequestCache cache, int hits, int misses) : ITrainingDueRequestStore
{
    public MemoryTrainingDueRequestStore Inner { get; set; } = inner;
    public TrainingDueRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueRequest? Find(string code)
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

    public void Save(TrainingDueRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due requests. Empty fields match all.</summary>
public sealed record TrainingDueRequestQuery(string Title, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueRequest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<TrainingDueRequest> Apply(List<TrainingDueRequest> items)
    {
        var matched = Collections.Filter(items, (TrainingDueRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due request store.</summary>
public static class TrainingDueRequestStore
{
    public static ITrainingDueRequestStore NewMemoryTrainingDueRequestStore()
    {
        return new MemoryTrainingDueRequestStore(new Dictionary<string, TrainingDueRequest>(), new List<string>());
    }

    public static ITrainingDueRequestStore NewCachedTrainingDueRequestStore(MemoryTrainingDueRequestStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueRequestStore(inner, TrainingDueRequestCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TrainingDueRequest> FindTrainingDueRequests(ITrainingDueRequestStore store, TrainingDueRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
