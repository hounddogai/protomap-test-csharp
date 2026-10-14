namespace Acme.Workforce.Talent.Mobility.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility scores by code.</summary>
public interface IMobilityScoreStore
{
    MobilityScore? Find(string code);
    void Save(MobilityScore item);
    bool Remove(string code);
    List<MobilityScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilityScoreStore(Dictionary<string, MobilityScore> items, List<string> order) : IMobilityScoreStore
{
    public Dictionary<string, MobilityScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilityScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilityScore item)
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

    public List<MobilityScore> List()
    {
        var outValue = new List<MobilityScore>();
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
public sealed class CachedMobilityScoreStore(MemoryMobilityScoreStore inner, MobilityScoreCache cache, int hits, int misses) : IMobilityScoreStore
{
    public MemoryMobilityScoreStore Inner { get; set; } = inner;
    public MobilityScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilityScore? Find(string code)
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

    public void Save(MobilityScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilityScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility scores. Empty fields match all.</summary>
public sealed record MobilityScoreQuery(string Title, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(MobilityScore item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MobilityScore> Apply(List<MobilityScore> items)
    {
        var matched = Collections.Filter(items, (MobilityScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility score store.</summary>
public static class MobilityScoreStore
{
    public static IMobilityScoreStore NewMemoryMobilityScoreStore()
    {
        return new MemoryMobilityScoreStore(new Dictionary<string, MobilityScore>(), new List<string>());
    }

    public static IMobilityScoreStore NewCachedMobilityScoreStore(MemoryMobilityScoreStore inner, int ttl, IClock clock)
    {
        return new CachedMobilityScoreStore(inner, MobilityScoreCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<MobilityScore> FindMobilityScores(IMobilityScoreStore store, MobilityScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
