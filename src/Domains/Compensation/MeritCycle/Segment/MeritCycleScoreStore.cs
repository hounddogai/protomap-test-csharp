namespace Acme.Workforce.Compensation.MeritCycle.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle scores by code.</summary>
public interface IMeritCycleScoreStore
{
    MeritCycleScore? Find(string code);
    void Save(MeritCycleScore item);
    bool Remove(string code);
    List<MeritCycleScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleScoreStore(Dictionary<string, MeritCycleScore> items, List<string> order) : IMeritCycleScoreStore
{
    public Dictionary<string, MeritCycleScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleScore item)
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

    public List<MeritCycleScore> List()
    {
        var outValue = new List<MeritCycleScore>();
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
public sealed class CachedMeritCycleScoreStore(MemoryMeritCycleScoreStore inner, MeritCycleScoreCache cache, int hits, int misses) : IMeritCycleScoreStore
{
    public MemoryMeritCycleScoreStore Inner { get; set; } = inner;
    public MeritCycleScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleScore? Find(string code)
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

    public void Save(MeritCycleScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle scores. Empty fields match all.</summary>
public sealed record MeritCycleScoreQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleScore item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleScore> Apply(List<MeritCycleScore> items)
    {
        var matched = Collections.Filter(items, (MeritCycleScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle score store.</summary>
public static class MeritCycleScoreStore
{
    public static IMeritCycleScoreStore NewMemoryMeritCycleScoreStore()
    {
        return new MemoryMeritCycleScoreStore(new Dictionary<string, MeritCycleScore>(), new List<string>());
    }

    public static IMeritCycleScoreStore NewCachedMeritCycleScoreStore(MemoryMeritCycleScoreStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleScoreStore(inner, MeritCycleScoreCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<MeritCycleScore> FindMeritCycleScores(IMeritCycleScoreStore store, MeritCycleScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
