namespace Acme.Workforce.Engagement.ActionPlan.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual action plan entries by code.</summary>
public interface IManualActionPlanEntryStore
{
    ManualActionPlanEntry? Find(string code);
    void Save(ManualActionPlanEntry item);
    bool Remove(string code);
    List<ManualActionPlanEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualActionPlanEntryStore(Dictionary<string, ManualActionPlanEntry> items, List<string> order) : IManualActionPlanEntryStore
{
    public Dictionary<string, ManualActionPlanEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualActionPlanEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualActionPlanEntry item)
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

    public List<ManualActionPlanEntry> List()
    {
        var outValue = new List<ManualActionPlanEntry>();
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
public sealed class CachedManualActionPlanEntryStore(MemoryManualActionPlanEntryStore inner, ManualActionPlanEntryCache cache, int hits, int misses) : IManualActionPlanEntryStore
{
    public MemoryManualActionPlanEntryStore Inner { get; set; } = inner;
    public ManualActionPlanEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualActionPlanEntry? Find(string code)
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

    public void Save(ManualActionPlanEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualActionPlanEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual action plan entries. Empty fields match all.</summary>
public sealed record ManualActionPlanEntryQuery(string Country, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(ManualActionPlanEntry item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualActionPlanEntry> Apply(List<ManualActionPlanEntry> items)
    {
        var matched = Collections.Filter(items, (ManualActionPlanEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual action plan entry store.</summary>
public static class ManualActionPlanEntryStore
{
    public static IManualActionPlanEntryStore NewMemoryManualActionPlanEntryStore()
    {
        return new MemoryManualActionPlanEntryStore(
            new Dictionary<string, ManualActionPlanEntry>(),
            new List<string>());
    }

    public static IManualActionPlanEntryStore NewCachedManualActionPlanEntryStore(MemoryManualActionPlanEntryStore inner, int ttl, IClock clock)
    {
        return new CachedManualActionPlanEntryStore(inner, ManualActionPlanEntryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ManualActionPlanEntry> FindManualActionPlanEntries(IManualActionPlanEntryStore store, ManualActionPlanEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
