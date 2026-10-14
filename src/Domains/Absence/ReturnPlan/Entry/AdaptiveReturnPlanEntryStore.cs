namespace Acme.Workforce.Absence.ReturnPlan.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive return plan entries by code.</summary>
public interface IAdaptiveReturnPlanEntryStore
{
    AdaptiveReturnPlanEntry? Find(string code);
    void Save(AdaptiveReturnPlanEntry item);
    bool Remove(string code);
    List<AdaptiveReturnPlanEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveReturnPlanEntryStore(Dictionary<string, AdaptiveReturnPlanEntry> items, List<string> order) : IAdaptiveReturnPlanEntryStore
{
    public Dictionary<string, AdaptiveReturnPlanEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveReturnPlanEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveReturnPlanEntry item)
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

    public List<AdaptiveReturnPlanEntry> List()
    {
        var outValue = new List<AdaptiveReturnPlanEntry>();
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
public sealed class CachedAdaptiveReturnPlanEntryStore(MemoryAdaptiveReturnPlanEntryStore inner, AdaptiveReturnPlanEntryCache cache, int hits, int misses) : IAdaptiveReturnPlanEntryStore
{
    public MemoryAdaptiveReturnPlanEntryStore Inner { get; set; } = inner;
    public AdaptiveReturnPlanEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveReturnPlanEntry? Find(string code)
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

    public void Save(AdaptiveReturnPlanEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveReturnPlanEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive return plan entries. Empty fields match all.</summary>
public sealed record AdaptiveReturnPlanEntryQuery(string Team, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveReturnPlanEntry item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<AdaptiveReturnPlanEntry> Apply(List<AdaptiveReturnPlanEntry> items)
    {
        var matched = Collections.Filter(items, (AdaptiveReturnPlanEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive return plan entry store.</summary>
public static class AdaptiveReturnPlanEntryStore
{
    public static IAdaptiveReturnPlanEntryStore NewMemoryAdaptiveReturnPlanEntryStore()
    {
        return new MemoryAdaptiveReturnPlanEntryStore(
            new Dictionary<string, AdaptiveReturnPlanEntry>(),
            new List<string>());
    }

    public static IAdaptiveReturnPlanEntryStore NewCachedAdaptiveReturnPlanEntryStore(MemoryAdaptiveReturnPlanEntryStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveReturnPlanEntryStore(
            inner,
            AdaptiveReturnPlanEntryCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveReturnPlanEntry> FindAdaptiveReturnPlanEntries(IAdaptiveReturnPlanEntryStore store, AdaptiveReturnPlanEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
