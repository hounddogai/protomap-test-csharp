namespace Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual scenario windows by code.</summary>
public interface IManualScenarioWindowStore
{
    ManualScenarioWindow? Find(string code);
    void Save(ManualScenarioWindow item);
    bool Remove(string code);
    List<ManualScenarioWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualScenarioWindowStore(Dictionary<string, ManualScenarioWindow> items, List<string> order) : IManualScenarioWindowStore
{
    public Dictionary<string, ManualScenarioWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualScenarioWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualScenarioWindow item)
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

    public List<ManualScenarioWindow> List()
    {
        var outValue = new List<ManualScenarioWindow>();
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
public sealed class CachedManualScenarioWindowStore(MemoryManualScenarioWindowStore inner, ManualScenarioWindowCache cache, int hits, int misses) : IManualScenarioWindowStore
{
    public MemoryManualScenarioWindowStore Inner { get; set; } = inner;
    public ManualScenarioWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualScenarioWindow? Find(string code)
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

    public void Save(ManualScenarioWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualScenarioWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual scenario windows. Empty fields match all.</summary>
public sealed record ManualScenarioWindowQuery(string Unit, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(ManualScenarioWindow item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<ManualScenarioWindow> Apply(List<ManualScenarioWindow> items)
    {
        var matched = Collections.Filter(items, (ManualScenarioWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual scenario window store.</summary>
public static class ManualScenarioWindowStore
{
    public static IManualScenarioWindowStore NewMemoryManualScenarioWindowStore()
    {
        return new MemoryManualScenarioWindowStore(
            new Dictionary<string, ManualScenarioWindow>(),
            new List<string>());
    }

    public static IManualScenarioWindowStore NewCachedManualScenarioWindowStore(MemoryManualScenarioWindowStore inner, int ttl, IClock clock)
    {
        return new CachedManualScenarioWindowStore(inner, ManualScenarioWindowCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ManualScenarioWindow> FindManualScenarioWindows(IManualScenarioWindowStore store, ManualScenarioWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
