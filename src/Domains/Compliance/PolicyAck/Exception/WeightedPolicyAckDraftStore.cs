namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted policy ack drafts by code.</summary>
public interface IWeightedPolicyAckDraftStore
{
    WeightedPolicyAckDraft? Find(string code);
    void Save(WeightedPolicyAckDraft item);
    bool Remove(string code);
    List<WeightedPolicyAckDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedPolicyAckDraftStore(Dictionary<string, WeightedPolicyAckDraft> items, List<string> order) : IWeightedPolicyAckDraftStore
{
    public Dictionary<string, WeightedPolicyAckDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedPolicyAckDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedPolicyAckDraft item)
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

    public List<WeightedPolicyAckDraft> List()
    {
        var outValue = new List<WeightedPolicyAckDraft>();
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
public sealed class CachedWeightedPolicyAckDraftStore(MemoryWeightedPolicyAckDraftStore inner, WeightedPolicyAckDraftCache cache, int hits, int misses) : IWeightedPolicyAckDraftStore
{
    public MemoryWeightedPolicyAckDraftStore Inner { get; set; } = inner;
    public WeightedPolicyAckDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedPolicyAckDraft? Find(string code)
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

    public void Save(WeightedPolicyAckDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedPolicyAckDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted policy ack drafts. Empty fields match all.</summary>
public sealed record WeightedPolicyAckDraftQuery(string Category, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedPolicyAckDraft item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<WeightedPolicyAckDraft> Apply(List<WeightedPolicyAckDraft> items)
    {
        var matched = Collections.Filter(items, (WeightedPolicyAckDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted policy ack draft store.</summary>
public static class WeightedPolicyAckDraftStore
{
    public static IWeightedPolicyAckDraftStore NewMemoryWeightedPolicyAckDraftStore()
    {
        return new MemoryWeightedPolicyAckDraftStore(
            new Dictionary<string, WeightedPolicyAckDraft>(),
            new List<string>());
    }

    public static IWeightedPolicyAckDraftStore NewCachedWeightedPolicyAckDraftStore(MemoryWeightedPolicyAckDraftStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedPolicyAckDraftStore(
            inner,
            WeightedPolicyAckDraftCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<WeightedPolicyAckDraft> FindWeightedPolicyAckDrafts(IWeightedPolicyAckDraftStore store, WeightedPolicyAckDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
