namespace Acme.Workforce.Contractors.Offboarding.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding windows by code.</summary>
public interface IOffboardingWindowStore
{
    OffboardingWindow? Find(string code);
    void Save(OffboardingWindow item);
    bool Remove(string code);
    List<OffboardingWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingWindowStore(Dictionary<string, OffboardingWindow> items, List<string> order) : IOffboardingWindowStore
{
    public Dictionary<string, OffboardingWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingWindow item)
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

    public List<OffboardingWindow> List()
    {
        var outValue = new List<OffboardingWindow>();
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
public sealed class CachedOffboardingWindowStore(MemoryOffboardingWindowStore inner, OffboardingWindowCache cache, int hits, int misses) : IOffboardingWindowStore
{
    public MemoryOffboardingWindowStore Inner { get; set; } = inner;
    public OffboardingWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingWindow? Find(string code)
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

    public void Save(OffboardingWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding windows. Empty fields match all.</summary>
public sealed record OffboardingWindowQuery(string Title, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingWindow item)
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

    public List<OffboardingWindow> Apply(List<OffboardingWindow> items)
    {
        var matched = Collections.Filter(items, (OffboardingWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding window store.</summary>
public static class OffboardingWindowStore
{
    public static IOffboardingWindowStore NewMemoryOffboardingWindowStore()
    {
        return new MemoryOffboardingWindowStore(new Dictionary<string, OffboardingWindow>(), new List<string>());
    }

    public static IOffboardingWindowStore NewCachedOffboardingWindowStore(MemoryOffboardingWindowStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingWindowStore(inner, OffboardingWindowCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<OffboardingWindow> FindOffboardingWindows(IOffboardingWindowStore store, OffboardingWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
