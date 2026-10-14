namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey scores by code.</summary>
public interface IPulseSurveyScoreStore
{
    PulseSurveyScore? Find(string code);
    void Save(PulseSurveyScore item);
    bool Remove(string code);
    List<PulseSurveyScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyScoreStore(Dictionary<string, PulseSurveyScore> items, List<string> order) : IPulseSurveyScoreStore
{
    public Dictionary<string, PulseSurveyScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyScore item)
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

    public List<PulseSurveyScore> List()
    {
        var outValue = new List<PulseSurveyScore>();
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
public sealed class CachedPulseSurveyScoreStore(MemoryPulseSurveyScoreStore inner, PulseSurveyScoreCache cache, int hits, int misses) : IPulseSurveyScoreStore
{
    public MemoryPulseSurveyScoreStore Inner { get; set; } = inner;
    public PulseSurveyScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyScore? Find(string code)
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

    public void Save(PulseSurveyScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey scores. Empty fields match all.</summary>
public sealed record PulseSurveyScoreQuery(string Unit, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyScore item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PulseSurveyScore> Apply(List<PulseSurveyScore> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey score store.</summary>
public static class PulseSurveyScoreStore
{
    public static IPulseSurveyScoreStore NewMemoryPulseSurveyScoreStore()
    {
        return new MemoryPulseSurveyScoreStore(new Dictionary<string, PulseSurveyScore>(), new List<string>());
    }

    public static IPulseSurveyScoreStore NewCachedPulseSurveyScoreStore(MemoryPulseSurveyScoreStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyScoreStore(inner, PulseSurveyScoreCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PulseSurveyScore> FindPulseSurveyScores(IPulseSurveyScoreStore store, PulseSurveyScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
