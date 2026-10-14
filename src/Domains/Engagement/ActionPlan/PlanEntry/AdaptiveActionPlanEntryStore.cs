namespace Acme.Workforce.Engagement.ActionPlan.PlanEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive action plan entries by code.</summary>
public interface IAdaptiveActionPlanEntryStore
{
    AdaptiveActionPlanEntry? Find(string code);
    void Save(AdaptiveActionPlanEntry item);
    bool Remove(string code);
    List<AdaptiveActionPlanEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveActionPlanEntryStore(Dictionary<string, AdaptiveActionPlanEntry> items, List<string> order) : IAdaptiveActionPlanEntryStore
{
    public Dictionary<string, AdaptiveActionPlanEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveActionPlanEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveActionPlanEntry item)
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

    public List<AdaptiveActionPlanEntry> List()
    {
        var outValue = new List<AdaptiveActionPlanEntry>();
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
public sealed class CachedAdaptiveActionPlanEntryStore(MemoryAdaptiveActionPlanEntryStore inner, AdaptiveActionPlanEntryCache cache, int hits, int misses) : IAdaptiveActionPlanEntryStore
{
    public MemoryAdaptiveActionPlanEntryStore Inner { get; set; } = inner;
    public AdaptiveActionPlanEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveActionPlanEntry? Find(string code)
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

    public void Save(AdaptiveActionPlanEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveActionPlanEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive action plan entries. Empty fields match all.</summary>
public sealed record AdaptiveActionPlanEntryQuery(string Label, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveActionPlanEntry item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveActionPlanEntry> Apply(List<AdaptiveActionPlanEntry> items)
    {
        var matched = Collections.Filter(items, (AdaptiveActionPlanEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive action plan entry store.</summary>
public static class AdaptiveActionPlanEntryStore
{
    public static IAdaptiveActionPlanEntryStore NewMemoryAdaptiveActionPlanEntryStore()
    {
        return new MemoryAdaptiveActionPlanEntryStore(
            new Dictionary<string, AdaptiveActionPlanEntry>(),
            new List<string>());
    }

    public static IAdaptiveActionPlanEntryStore NewCachedAdaptiveActionPlanEntryStore(MemoryAdaptiveActionPlanEntryStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveActionPlanEntryStore(
            inner,
            AdaptiveActionPlanEntryCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveActionPlanEntry> FindAdaptiveActionPlanEntries(IAdaptiveActionPlanEntryStore store, AdaptiveActionPlanEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
