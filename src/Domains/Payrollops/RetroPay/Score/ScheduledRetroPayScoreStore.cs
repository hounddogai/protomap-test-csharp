namespace Acme.Workforce.Payrollops.RetroPay.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled retro pay scores by code.</summary>
public interface IScheduledRetroPayScoreStore
{
    ScheduledRetroPayScore? Find(string code);
    void Save(ScheduledRetroPayScore item);
    bool Remove(string code);
    List<ScheduledRetroPayScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledRetroPayScoreStore(Dictionary<string, ScheduledRetroPayScore> items, List<string> order) : IScheduledRetroPayScoreStore
{
    public Dictionary<string, ScheduledRetroPayScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledRetroPayScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledRetroPayScore item)
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

    public List<ScheduledRetroPayScore> List()
    {
        var outValue = new List<ScheduledRetroPayScore>();
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
public sealed class CachedScheduledRetroPayScoreStore(MemoryScheduledRetroPayScoreStore inner, ScheduledRetroPayScoreCache cache, int hits, int misses) : IScheduledRetroPayScoreStore
{
    public MemoryScheduledRetroPayScoreStore Inner { get; set; } = inner;
    public ScheduledRetroPayScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledRetroPayScore? Find(string code)
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

    public void Save(ScheduledRetroPayScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledRetroPayScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled retro pay scores. Empty fields match all.</summary>
public sealed record ScheduledRetroPayScoreQuery(string Unit, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledRetroPayScore item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledRetroPayScore> Apply(List<ScheduledRetroPayScore> items)
    {
        var matched = Collections.Filter(items, (ScheduledRetroPayScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled retro pay score store.</summary>
public static class ScheduledRetroPayScoreStore
{
    public static IScheduledRetroPayScoreStore NewMemoryScheduledRetroPayScoreStore()
    {
        return new MemoryScheduledRetroPayScoreStore(
            new Dictionary<string, ScheduledRetroPayScore>(),
            new List<string>());
    }

    public static IScheduledRetroPayScoreStore NewCachedScheduledRetroPayScoreStore(MemoryScheduledRetroPayScoreStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledRetroPayScoreStore(
            inner,
            ScheduledRetroPayScoreCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledRetroPayScore> FindScheduledRetroPayScores(IScheduledRetroPayScoreStore store, ScheduledRetroPayScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
