namespace Acme.Workforce.Orgdesign.Headcount.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount scores by code.</summary>
public interface IHeadcountScoreStore
{
    HeadcountScore? Find(string code);
    void Save(HeadcountScore item);
    bool Remove(string code);
    List<HeadcountScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountScoreStore(Dictionary<string, HeadcountScore> items, List<string> order) : IHeadcountScoreStore
{
    public Dictionary<string, HeadcountScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountScore item)
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

    public List<HeadcountScore> List()
    {
        var outValue = new List<HeadcountScore>();
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
public sealed class CachedHeadcountScoreStore(MemoryHeadcountScoreStore inner, HeadcountScoreCache cache, int hits, int misses) : IHeadcountScoreStore
{
    public MemoryHeadcountScoreStore Inner { get; set; } = inner;
    public HeadcountScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountScore? Find(string code)
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

    public void Save(HeadcountScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount scores. Empty fields match all.</summary>
public sealed record HeadcountScoreQuery(string Source, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountScore item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountScore> Apply(List<HeadcountScore> items)
    {
        var matched = Collections.Filter(items, (HeadcountScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount score store.</summary>
public static class HeadcountScoreStore
{
    public static IHeadcountScoreStore NewMemoryHeadcountScoreStore()
    {
        return new MemoryHeadcountScoreStore(new Dictionary<string, HeadcountScore>(), new List<string>());
    }

    public static IHeadcountScoreStore NewCachedHeadcountScoreStore(MemoryHeadcountScoreStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountScoreStore(inner, HeadcountScoreCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<HeadcountScore> FindHeadcountScores(IHeadcountScoreStore store, HeadcountScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
