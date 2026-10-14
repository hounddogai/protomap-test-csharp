namespace Acme.Workforce.Talent.SuccessionPlan.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan windows by code.</summary>
public interface ISuccessionPlanWindowStore
{
    SuccessionPlanWindow? Find(string code);
    void Save(SuccessionPlanWindow item);
    bool Remove(string code);
    List<SuccessionPlanWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanWindowStore(Dictionary<string, SuccessionPlanWindow> items, List<string> order) : ISuccessionPlanWindowStore
{
    public Dictionary<string, SuccessionPlanWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanWindow item)
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

    public List<SuccessionPlanWindow> List()
    {
        var outValue = new List<SuccessionPlanWindow>();
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
public sealed class CachedSuccessionPlanWindowStore(MemorySuccessionPlanWindowStore inner, SuccessionPlanWindowCache cache, int hits, int misses) : ISuccessionPlanWindowStore
{
    public MemorySuccessionPlanWindowStore Inner { get; set; } = inner;
    public SuccessionPlanWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanWindow? Find(string code)
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

    public void Save(SuccessionPlanWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan windows. Empty fields match all.</summary>
public sealed record SuccessionPlanWindowQuery(string Locale, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanWindow item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SuccessionPlanWindow> Apply(List<SuccessionPlanWindow> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan window store.</summary>
public static class SuccessionPlanWindowStore
{
    public static ISuccessionPlanWindowStore NewMemorySuccessionPlanWindowStore()
    {
        return new MemorySuccessionPlanWindowStore(
            new Dictionary<string, SuccessionPlanWindow>(),
            new List<string>());
    }

    public static ISuccessionPlanWindowStore NewCachedSuccessionPlanWindowStore(MemorySuccessionPlanWindowStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanWindowStore(inner, SuccessionPlanWindowCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<SuccessionPlanWindow> FindSuccessionPlanWindows(ISuccessionPlanWindowStore store, SuccessionPlanWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
