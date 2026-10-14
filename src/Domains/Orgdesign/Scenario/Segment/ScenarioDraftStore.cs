namespace Acme.Workforce.Orgdesign.Scenario.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario drafts by code.</summary>
public interface IScenarioDraftStore
{
    ScenarioDraft? Find(string code);
    void Save(ScenarioDraft item);
    bool Remove(string code);
    List<ScenarioDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioDraftStore(Dictionary<string, ScenarioDraft> items, List<string> order) : IScenarioDraftStore
{
    public Dictionary<string, ScenarioDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioDraft item)
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

    public List<ScenarioDraft> List()
    {
        var outValue = new List<ScenarioDraft>();
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
public sealed class CachedScenarioDraftStore(MemoryScenarioDraftStore inner, ScenarioDraftCache cache, int hits, int misses) : IScenarioDraftStore
{
    public MemoryScenarioDraftStore Inner { get; set; } = inner;
    public ScenarioDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioDraft? Find(string code)
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

    public void Save(ScenarioDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario drafts. Empty fields match all.</summary>
public sealed record ScenarioDraftQuery(string Team, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioDraft item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScenarioDraft> Apply(List<ScenarioDraft> items)
    {
        var matched = Collections.Filter(items, (ScenarioDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario draft store.</summary>
public static class ScenarioDraftStore
{
    public static IScenarioDraftStore NewMemoryScenarioDraftStore()
    {
        return new MemoryScenarioDraftStore(new Dictionary<string, ScenarioDraft>(), new List<string>());
    }

    public static IScenarioDraftStore NewCachedScenarioDraftStore(MemoryScenarioDraftStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioDraftStore(inner, ScenarioDraftCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ScenarioDraft> FindScenarioDrafts(IScenarioDraftStore store, ScenarioDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
