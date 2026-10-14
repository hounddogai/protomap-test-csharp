namespace Acme.Workforce.Absence.ReturnPlan.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan entries by code.</summary>
public interface IReturnPlanEntryStore
{
    ReturnPlanEntry? Find(string code);
    void Save(ReturnPlanEntry item);
    bool Remove(string code);
    List<ReturnPlanEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanEntryStore(Dictionary<string, ReturnPlanEntry> items, List<string> order) : IReturnPlanEntryStore
{
    public Dictionary<string, ReturnPlanEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanEntry item)
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

    public List<ReturnPlanEntry> List()
    {
        var outValue = new List<ReturnPlanEntry>();
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
public sealed class CachedReturnPlanEntryStore(MemoryReturnPlanEntryStore inner, ReturnPlanEntryCache cache, int hits, int misses) : IReturnPlanEntryStore
{
    public MemoryReturnPlanEntryStore Inner { get; set; } = inner;
    public ReturnPlanEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanEntry? Find(string code)
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

    public void Save(ReturnPlanEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan entries. Empty fields match all.</summary>
public sealed record ReturnPlanEntryQuery(string Title, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanEntry item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<ReturnPlanEntry> Apply(List<ReturnPlanEntry> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan entry store.</summary>
public static class ReturnPlanEntryStore
{
    public static IReturnPlanEntryStore NewMemoryReturnPlanEntryStore()
    {
        return new MemoryReturnPlanEntryStore(new Dictionary<string, ReturnPlanEntry>(), new List<string>());
    }

    public static IReturnPlanEntryStore NewCachedReturnPlanEntryStore(MemoryReturnPlanEntryStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanEntryStore(inner, ReturnPlanEntryCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanEntry> FindReturnPlanEntries(IReturnPlanEntryStore store, ReturnPlanEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
