namespace Acme.Workforce.Payrollops.Garnishment.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment scores by code.</summary>
public interface IGarnishmentScoreStore
{
    GarnishmentScore? Find(string code);
    void Save(GarnishmentScore item);
    bool Remove(string code);
    List<GarnishmentScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentScoreStore(Dictionary<string, GarnishmentScore> items, List<string> order) : IGarnishmentScoreStore
{
    public Dictionary<string, GarnishmentScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentScore item)
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

    public List<GarnishmentScore> List()
    {
        var outValue = new List<GarnishmentScore>();
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
public sealed class CachedGarnishmentScoreStore(MemoryGarnishmentScoreStore inner, GarnishmentScoreCache cache, int hits, int misses) : IGarnishmentScoreStore
{
    public MemoryGarnishmentScoreStore Inner { get; set; } = inner;
    public GarnishmentScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentScore? Find(string code)
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

    public void Save(GarnishmentScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment scores. Empty fields match all.</summary>
public sealed record GarnishmentScoreQuery(string Name, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentScore item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GarnishmentScore> Apply(List<GarnishmentScore> items)
    {
        var matched = Collections.Filter(items, (GarnishmentScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment score store.</summary>
public static class GarnishmentScoreStore
{
    public static IGarnishmentScoreStore NewMemoryGarnishmentScoreStore()
    {
        return new MemoryGarnishmentScoreStore(new Dictionary<string, GarnishmentScore>(), new List<string>());
    }

    public static IGarnishmentScoreStore NewCachedGarnishmentScoreStore(MemoryGarnishmentScoreStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentScoreStore(inner, GarnishmentScoreCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<GarnishmentScore> FindGarnishmentScores(IGarnishmentScoreStore store, GarnishmentScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
