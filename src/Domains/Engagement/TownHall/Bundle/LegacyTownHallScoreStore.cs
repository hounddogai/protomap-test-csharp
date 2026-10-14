namespace Acme.Workforce.Engagement.TownHall.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy town hall scores by code.</summary>
public interface ILegacyTownHallScoreStore
{
    LegacyTownHallScore? Find(string code);
    void Save(LegacyTownHallScore item);
    bool Remove(string code);
    List<LegacyTownHallScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyTownHallScoreStore(Dictionary<string, LegacyTownHallScore> items, List<string> order) : ILegacyTownHallScoreStore
{
    public Dictionary<string, LegacyTownHallScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyTownHallScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyTownHallScore item)
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

    public List<LegacyTownHallScore> List()
    {
        var outValue = new List<LegacyTownHallScore>();
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
public sealed class CachedLegacyTownHallScoreStore(MemoryLegacyTownHallScoreStore inner, LegacyTownHallScoreCache cache, int hits, int misses) : ILegacyTownHallScoreStore
{
    public MemoryLegacyTownHallScoreStore Inner { get; set; } = inner;
    public LegacyTownHallScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyTownHallScore? Find(string code)
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

    public void Save(LegacyTownHallScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyTownHallScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy town hall scores. Empty fields match all.</summary>
public sealed record LegacyTownHallScoreQuery(string Country, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyTownHallScore item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyTownHallScore> Apply(List<LegacyTownHallScore> items)
    {
        var matched = Collections.Filter(items, (LegacyTownHallScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy town hall score store.</summary>
public static class LegacyTownHallScoreStore
{
    public static ILegacyTownHallScoreStore NewMemoryLegacyTownHallScoreStore()
    {
        return new MemoryLegacyTownHallScoreStore(new Dictionary<string, LegacyTownHallScore>(), new List<string>());
    }

    public static ILegacyTownHallScoreStore NewCachedLegacyTownHallScoreStore(MemoryLegacyTownHallScoreStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyTownHallScoreStore(inner, LegacyTownHallScoreCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LegacyTownHallScore> FindLegacyTownHallScores(ILegacyTownHallScoreStore store, LegacyTownHallScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
