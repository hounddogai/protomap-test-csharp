namespace Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default scenario windows by code.</summary>
public interface IDefaultScenarioWindowStore
{
    DefaultScenarioWindow? Find(string code);
    void Save(DefaultScenarioWindow item);
    bool Remove(string code);
    List<DefaultScenarioWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultScenarioWindowStore(Dictionary<string, DefaultScenarioWindow> items, List<string> order) : IDefaultScenarioWindowStore
{
    public Dictionary<string, DefaultScenarioWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultScenarioWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultScenarioWindow item)
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

    public List<DefaultScenarioWindow> List()
    {
        var outValue = new List<DefaultScenarioWindow>();
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
public sealed class CachedDefaultScenarioWindowStore(MemoryDefaultScenarioWindowStore inner, DefaultScenarioWindowCache cache, int hits, int misses) : IDefaultScenarioWindowStore
{
    public MemoryDefaultScenarioWindowStore Inner { get; set; } = inner;
    public DefaultScenarioWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultScenarioWindow? Find(string code)
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

    public void Save(DefaultScenarioWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultScenarioWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default scenario windows. Empty fields match all.</summary>
public sealed record DefaultScenarioWindowQuery(string Label, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultScenarioWindow item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<DefaultScenarioWindow> Apply(List<DefaultScenarioWindow> items)
    {
        var matched = Collections.Filter(items, (DefaultScenarioWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default scenario window store.</summary>
public static class DefaultScenarioWindowStore
{
    public static IDefaultScenarioWindowStore NewMemoryDefaultScenarioWindowStore()
    {
        return new MemoryDefaultScenarioWindowStore(
            new Dictionary<string, DefaultScenarioWindow>(),
            new List<string>());
    }

    public static IDefaultScenarioWindowStore NewCachedDefaultScenarioWindowStore(MemoryDefaultScenarioWindowStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultScenarioWindowStore(inner, DefaultScenarioWindowCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<DefaultScenarioWindow> FindDefaultScenarioWindows(IDefaultScenarioWindowStore store, DefaultScenarioWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
