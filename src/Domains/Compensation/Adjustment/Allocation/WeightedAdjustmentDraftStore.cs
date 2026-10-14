namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted adjustment drafts by code.</summary>
public interface IWeightedAdjustmentDraftStore
{
    WeightedAdjustmentDraft? Find(string code);
    void Save(WeightedAdjustmentDraft item);
    bool Remove(string code);
    List<WeightedAdjustmentDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedAdjustmentDraftStore(Dictionary<string, WeightedAdjustmentDraft> items, List<string> order) : IWeightedAdjustmentDraftStore
{
    public Dictionary<string, WeightedAdjustmentDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedAdjustmentDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedAdjustmentDraft item)
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

    public List<WeightedAdjustmentDraft> List()
    {
        var outValue = new List<WeightedAdjustmentDraft>();
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
public sealed class CachedWeightedAdjustmentDraftStore(MemoryWeightedAdjustmentDraftStore inner, WeightedAdjustmentDraftCache cache, int hits, int misses) : IWeightedAdjustmentDraftStore
{
    public MemoryWeightedAdjustmentDraftStore Inner { get; set; } = inner;
    public WeightedAdjustmentDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedAdjustmentDraft? Find(string code)
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

    public void Save(WeightedAdjustmentDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedAdjustmentDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted adjustment drafts. Empty fields match all.</summary>
public sealed record WeightedAdjustmentDraftQuery(string Label, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedAdjustmentDraft item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedAdjustmentDraft> Apply(List<WeightedAdjustmentDraft> items)
    {
        var matched = Collections.Filter(items, (WeightedAdjustmentDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted adjustment draft store.</summary>
public static class WeightedAdjustmentDraftStore
{
    public static IWeightedAdjustmentDraftStore NewMemoryWeightedAdjustmentDraftStore()
    {
        return new MemoryWeightedAdjustmentDraftStore(
            new Dictionary<string, WeightedAdjustmentDraft>(),
            new List<string>());
    }

    public static IWeightedAdjustmentDraftStore NewCachedWeightedAdjustmentDraftStore(MemoryWeightedAdjustmentDraftStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedAdjustmentDraftStore(
            inner,
            WeightedAdjustmentDraftCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<WeightedAdjustmentDraft> FindWeightedAdjustmentDrafts(IWeightedAdjustmentDraftStore store, WeightedAdjustmentDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
