namespace Acme.Workforce.Assets.Warranty.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted warranty drafts by code.</summary>
public interface IWeightedWarrantyDraftStore
{
    WeightedWarrantyDraft? Find(string code);
    void Save(WeightedWarrantyDraft item);
    bool Remove(string code);
    List<WeightedWarrantyDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedWarrantyDraftStore(Dictionary<string, WeightedWarrantyDraft> items, List<string> order) : IWeightedWarrantyDraftStore
{
    public Dictionary<string, WeightedWarrantyDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedWarrantyDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedWarrantyDraft item)
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

    public List<WeightedWarrantyDraft> List()
    {
        var outValue = new List<WeightedWarrantyDraft>();
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
public sealed class CachedWeightedWarrantyDraftStore(MemoryWeightedWarrantyDraftStore inner, WeightedWarrantyDraftCache cache, int hits, int misses) : IWeightedWarrantyDraftStore
{
    public MemoryWeightedWarrantyDraftStore Inner { get; set; } = inner;
    public WeightedWarrantyDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedWarrantyDraft? Find(string code)
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

    public void Save(WeightedWarrantyDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedWarrantyDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted warranty drafts. Empty fields match all.</summary>
public sealed record WeightedWarrantyDraftQuery(string Title, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedWarrantyDraft item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedWarrantyDraft> Apply(List<WeightedWarrantyDraft> items)
    {
        var matched = Collections.Filter(items, (WeightedWarrantyDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted warranty draft store.</summary>
public static class WeightedWarrantyDraftStore
{
    public static IWeightedWarrantyDraftStore NewMemoryWeightedWarrantyDraftStore()
    {
        return new MemoryWeightedWarrantyDraftStore(
            new Dictionary<string, WeightedWarrantyDraft>(),
            new List<string>());
    }

    public static IWeightedWarrantyDraftStore NewCachedWeightedWarrantyDraftStore(MemoryWeightedWarrantyDraftStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedWarrantyDraftStore(inner, WeightedWarrantyDraftCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<WeightedWarrantyDraft> FindWeightedWarrantyDrafts(IWeightedWarrantyDraftStore store, WeightedWarrantyDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
