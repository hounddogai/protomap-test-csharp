namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due policies by code.</summary>
public interface ITrainingDuePolicyStore
{
    TrainingDuePolicy? Find(string code);
    void Save(TrainingDuePolicy item);
    bool Remove(string code);
    List<TrainingDuePolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDuePolicyStore(Dictionary<string, TrainingDuePolicy> items, List<string> order) : ITrainingDuePolicyStore
{
    public Dictionary<string, TrainingDuePolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDuePolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDuePolicy item)
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

    public List<TrainingDuePolicy> List()
    {
        var outValue = new List<TrainingDuePolicy>();
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
public sealed class CachedTrainingDuePolicyStore(MemoryTrainingDuePolicyStore inner, TrainingDuePolicyCache cache, int hits, int misses) : ITrainingDuePolicyStore
{
    public MemoryTrainingDuePolicyStore Inner { get; set; } = inner;
    public TrainingDuePolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDuePolicy? Find(string code)
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

    public void Save(TrainingDuePolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDuePolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due policies. Empty fields match all.</summary>
public sealed record TrainingDuePolicyQuery(string Channel, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDuePolicy item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TrainingDuePolicy> Apply(List<TrainingDuePolicy> items)
    {
        var matched = Collections.Filter(items, (TrainingDuePolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due policy store.</summary>
public static class TrainingDuePolicyStore
{
    public static ITrainingDuePolicyStore NewMemoryTrainingDuePolicyStore()
    {
        return new MemoryTrainingDuePolicyStore(new Dictionary<string, TrainingDuePolicy>(), new List<string>());
    }

    public static ITrainingDuePolicyStore NewCachedTrainingDuePolicyStore(MemoryTrainingDuePolicyStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDuePolicyStore(inner, TrainingDuePolicyCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TrainingDuePolicy> FindTrainingDuePolicies(ITrainingDuePolicyStore store, TrainingDuePolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
