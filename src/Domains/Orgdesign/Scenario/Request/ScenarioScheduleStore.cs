namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario schedules by code.</summary>
public interface IScenarioScheduleStore
{
    ScenarioSchedule? Find(string code);
    void Save(ScenarioSchedule item);
    bool Remove(string code);
    List<ScenarioSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioScheduleStore(Dictionary<string, ScenarioSchedule> items, List<string> order) : IScenarioScheduleStore
{
    public Dictionary<string, ScenarioSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioSchedule item)
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

    public List<ScenarioSchedule> List()
    {
        var outValue = new List<ScenarioSchedule>();
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
public sealed class CachedScenarioScheduleStore(MemoryScenarioScheduleStore inner, ScenarioScheduleCache cache, int hits, int misses) : IScenarioScheduleStore
{
    public MemoryScenarioScheduleStore Inner { get; set; } = inner;
    public ScenarioScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioSchedule? Find(string code)
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

    public void Save(ScenarioSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario schedules. Empty fields match all.</summary>
public sealed record ScenarioScheduleQuery(string Unit, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioSchedule item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScenarioSchedule> Apply(List<ScenarioSchedule> items)
    {
        var matched = Collections.Filter(items, (ScenarioSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario schedule store.</summary>
public static class ScenarioScheduleStore
{
    public static IScenarioScheduleStore NewMemoryScenarioScheduleStore()
    {
        return new MemoryScenarioScheduleStore(new Dictionary<string, ScenarioSchedule>(), new List<string>());
    }

    public static IScenarioScheduleStore NewCachedScenarioScheduleStore(MemoryScenarioScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioScheduleStore(inner, ScenarioScheduleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ScenarioSchedule> FindScenarioSchedules(IScenarioScheduleStore store, ScenarioScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
