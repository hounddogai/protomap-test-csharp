namespace Acme.Workforce.Engagement.PulseSurvey.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey batches by code.</summary>
public interface IPulseSurveyBatchStore
{
    PulseSurveyBatch? Find(string code);
    void Save(PulseSurveyBatch item);
    bool Remove(string code);
    List<PulseSurveyBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyBatchStore(Dictionary<string, PulseSurveyBatch> items, List<string> order) : IPulseSurveyBatchStore
{
    public Dictionary<string, PulseSurveyBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyBatch item)
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

    public List<PulseSurveyBatch> List()
    {
        var outValue = new List<PulseSurveyBatch>();
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
public sealed class CachedPulseSurveyBatchStore(MemoryPulseSurveyBatchStore inner, PulseSurveyBatchCache cache, int hits, int misses) : IPulseSurveyBatchStore
{
    public MemoryPulseSurveyBatchStore Inner { get; set; } = inner;
    public PulseSurveyBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyBatch? Find(string code)
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

    public void Save(PulseSurveyBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey batches. Empty fields match all.</summary>
public sealed record PulseSurveyBatchQuery(string Region, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyBatch item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<PulseSurveyBatch> Apply(List<PulseSurveyBatch> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey batch store.</summary>
public static class PulseSurveyBatchStore
{
    public static IPulseSurveyBatchStore NewMemoryPulseSurveyBatchStore()
    {
        return new MemoryPulseSurveyBatchStore(new Dictionary<string, PulseSurveyBatch>(), new List<string>());
    }

    public static IPulseSurveyBatchStore NewCachedPulseSurveyBatchStore(MemoryPulseSurveyBatchStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyBatchStore(inner, PulseSurveyBatchCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PulseSurveyBatch> FindPulseSurveyBatches(IPulseSurveyBatchStore store, PulseSurveyBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
