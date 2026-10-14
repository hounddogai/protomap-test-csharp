namespace Acme.Workforce.Compliance.TrainingDue.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due drafts by code.</summary>
public interface ITrainingDueDraftStore
{
    TrainingDueDraft? Find(string code);
    void Save(TrainingDueDraft item);
    bool Remove(string code);
    List<TrainingDueDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueDraftStore(Dictionary<string, TrainingDueDraft> items, List<string> order) : ITrainingDueDraftStore
{
    public Dictionary<string, TrainingDueDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueDraft item)
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

    public List<TrainingDueDraft> List()
    {
        var outValue = new List<TrainingDueDraft>();
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
public sealed class CachedTrainingDueDraftStore(MemoryTrainingDueDraftStore inner, TrainingDueDraftCache cache, int hits, int misses) : ITrainingDueDraftStore
{
    public MemoryTrainingDueDraftStore Inner { get; set; } = inner;
    public TrainingDueDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueDraft? Find(string code)
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

    public void Save(TrainingDueDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due drafts. Empty fields match all.</summary>
public sealed record TrainingDueDraftQuery(string Title, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueDraft item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TrainingDueDraft> Apply(List<TrainingDueDraft> items)
    {
        var matched = Collections.Filter(items, (TrainingDueDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due draft store.</summary>
public static class TrainingDueDraftStore
{
    public static ITrainingDueDraftStore NewMemoryTrainingDueDraftStore()
    {
        return new MemoryTrainingDueDraftStore(new Dictionary<string, TrainingDueDraft>(), new List<string>());
    }

    public static ITrainingDueDraftStore NewCachedTrainingDueDraftStore(MemoryTrainingDueDraftStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueDraftStore(inner, TrainingDueDraftCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TrainingDueDraft> FindTrainingDueDrafts(ITrainingDueDraftStore store, TrainingDueDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
