namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay scores by code.</summary>
public interface IRetroPayScoreStore
{
    RetroPayScore? Find(string code);
    void Save(RetroPayScore item);
    bool Remove(string code);
    List<RetroPayScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayScoreStore(Dictionary<string, RetroPayScore> items, List<string> order) : IRetroPayScoreStore
{
    public Dictionary<string, RetroPayScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayScore item)
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

    public List<RetroPayScore> List()
    {
        var outValue = new List<RetroPayScore>();
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
public sealed class CachedRetroPayScoreStore(MemoryRetroPayScoreStore inner, RetroPayScoreCache cache, int hits, int misses) : IRetroPayScoreStore
{
    public MemoryRetroPayScoreStore Inner { get; set; } = inner;
    public RetroPayScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayScore? Find(string code)
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

    public void Save(RetroPayScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay scores. Empty fields match all.</summary>
public sealed record RetroPayScoreQuery(string Team, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayScore item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RetroPayScore> Apply(List<RetroPayScore> items)
    {
        var matched = Collections.Filter(items, (RetroPayScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay score store.</summary>
public static class RetroPayScoreStore
{
    public static IRetroPayScoreStore NewMemoryRetroPayScoreStore()
    {
        return new MemoryRetroPayScoreStore(new Dictionary<string, RetroPayScore>(), new List<string>());
    }

    public static IRetroPayScoreStore NewCachedRetroPayScoreStore(MemoryRetroPayScoreStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayScoreStore(inner, RetroPayScoreCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RetroPayScore> FindRetroPayScores(IRetroPayScoreStore store, RetroPayScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
