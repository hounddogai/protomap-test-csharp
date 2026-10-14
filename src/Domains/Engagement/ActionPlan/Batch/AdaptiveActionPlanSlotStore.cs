namespace Acme.Workforce.Engagement.ActionPlan.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive action plan slots by code.</summary>
public interface IAdaptiveActionPlanSlotStore
{
    AdaptiveActionPlanSlot? Find(string code);
    void Save(AdaptiveActionPlanSlot item);
    bool Remove(string code);
    List<AdaptiveActionPlanSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveActionPlanSlotStore(Dictionary<string, AdaptiveActionPlanSlot> items, List<string> order) : IAdaptiveActionPlanSlotStore
{
    public Dictionary<string, AdaptiveActionPlanSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveActionPlanSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveActionPlanSlot item)
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

    public List<AdaptiveActionPlanSlot> List()
    {
        var outValue = new List<AdaptiveActionPlanSlot>();
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
public sealed class CachedAdaptiveActionPlanSlotStore(MemoryAdaptiveActionPlanSlotStore inner, AdaptiveActionPlanSlotCache cache, int hits, int misses) : IAdaptiveActionPlanSlotStore
{
    public MemoryAdaptiveActionPlanSlotStore Inner { get; set; } = inner;
    public AdaptiveActionPlanSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveActionPlanSlot? Find(string code)
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

    public void Save(AdaptiveActionPlanSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveActionPlanSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive action plan slots. Empty fields match all.</summary>
public sealed record AdaptiveActionPlanSlotQuery(string Title, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveActionPlanSlot item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveActionPlanSlot> Apply(List<AdaptiveActionPlanSlot> items)
    {
        var matched = Collections.Filter(items, (AdaptiveActionPlanSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive action plan slot store.</summary>
public static class AdaptiveActionPlanSlotStore
{
    public static IAdaptiveActionPlanSlotStore NewMemoryAdaptiveActionPlanSlotStore()
    {
        return new MemoryAdaptiveActionPlanSlotStore(
            new Dictionary<string, AdaptiveActionPlanSlot>(),
            new List<string>());
    }

    public static IAdaptiveActionPlanSlotStore NewCachedAdaptiveActionPlanSlotStore(MemoryAdaptiveActionPlanSlotStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveActionPlanSlotStore(
            inner,
            AdaptiveActionPlanSlotCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveActionPlanSlot> FindAdaptiveActionPlanSlots(IAdaptiveActionPlanSlotStore store, AdaptiveActionPlanSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
