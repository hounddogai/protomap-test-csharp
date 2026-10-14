namespace Acme.Workforce.Absence.Policy.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled policy items by code.</summary>
public interface IScheduledPolicyItemStore
{
    ScheduledPolicyItem? Find(string code);
    void Save(ScheduledPolicyItem item);
    bool Remove(string code);
    List<ScheduledPolicyItem> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledPolicyItemStore(Dictionary<string, ScheduledPolicyItem> items, List<string> order) : IScheduledPolicyItemStore
{
    public Dictionary<string, ScheduledPolicyItem> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledPolicyItem? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledPolicyItem item)
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

    public List<ScheduledPolicyItem> List()
    {
        var outValue = new List<ScheduledPolicyItem>();
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
public sealed class CachedScheduledPolicyItemStore(MemoryScheduledPolicyItemStore inner, ScheduledPolicyItemCache cache, int hits, int misses) : IScheduledPolicyItemStore
{
    public MemoryScheduledPolicyItemStore Inner { get; set; } = inner;
    public ScheduledPolicyItemCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledPolicyItem? Find(string code)
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

    public void Save(ScheduledPolicyItem item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledPolicyItem> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled policy items. Empty fields match all.</summary>
public sealed record ScheduledPolicyItemQuery(string Category, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledPolicyItem item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledPolicyItem> Apply(List<ScheduledPolicyItem> items)
    {
        var matched = Collections.Filter(items, (ScheduledPolicyItem candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled policy item store.</summary>
public static class ScheduledPolicyItemStore
{
    public static IScheduledPolicyItemStore NewMemoryScheduledPolicyItemStore()
    {
        return new MemoryScheduledPolicyItemStore(new Dictionary<string, ScheduledPolicyItem>(), new List<string>());
    }

    public static IScheduledPolicyItemStore NewCachedScheduledPolicyItemStore(MemoryScheduledPolicyItemStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledPolicyItemStore(inner, ScheduledPolicyItemCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ScheduledPolicyItem> FindScheduledPolicyItems(IScheduledPolicyItemStore store, ScheduledPolicyItemQuery query)
    {
        return query.Apply(store.List());
    }
}
