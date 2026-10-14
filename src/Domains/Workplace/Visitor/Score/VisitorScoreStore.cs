namespace Acme.Workforce.Workplace.Visitor.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor scores by code.</summary>
public interface IVisitorScoreStore
{
    VisitorScore? Find(string code);
    void Save(VisitorScore item);
    bool Remove(string code);
    List<VisitorScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorScoreStore(Dictionary<string, VisitorScore> items, List<string> order) : IVisitorScoreStore
{
    public Dictionary<string, VisitorScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorScore item)
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

    public List<VisitorScore> List()
    {
        var outValue = new List<VisitorScore>();
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
public sealed class CachedVisitorScoreStore(MemoryVisitorScoreStore inner, VisitorScoreCache cache, int hits, int misses) : IVisitorScoreStore
{
    public MemoryVisitorScoreStore Inner { get; set; } = inner;
    public VisitorScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorScore? Find(string code)
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

    public void Save(VisitorScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor scores. Empty fields match all.</summary>
public sealed record VisitorScoreQuery(string Name, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorScore item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorScore> Apply(List<VisitorScore> items)
    {
        var matched = Collections.Filter(items, (VisitorScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor score store.</summary>
public static class VisitorScoreStore
{
    public static IVisitorScoreStore NewMemoryVisitorScoreStore()
    {
        return new MemoryVisitorScoreStore(new Dictionary<string, VisitorScore>(), new List<string>());
    }

    public static IVisitorScoreStore NewCachedVisitorScoreStore(MemoryVisitorScoreStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorScoreStore(inner, VisitorScoreCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<VisitorScore> FindVisitorScores(IVisitorScoreStore store, VisitorScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
