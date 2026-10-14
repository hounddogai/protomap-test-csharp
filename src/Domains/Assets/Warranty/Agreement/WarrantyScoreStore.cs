namespace Acme.Workforce.Assets.Warranty.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty scores by code.</summary>
public interface IWarrantyScoreStore
{
    WarrantyScore? Find(string code);
    void Save(WarrantyScore item);
    bool Remove(string code);
    List<WarrantyScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyScoreStore(Dictionary<string, WarrantyScore> items, List<string> order) : IWarrantyScoreStore
{
    public Dictionary<string, WarrantyScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyScore item)
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

    public List<WarrantyScore> List()
    {
        var outValue = new List<WarrantyScore>();
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
public sealed class CachedWarrantyScoreStore(MemoryWarrantyScoreStore inner, WarrantyScoreCache cache, int hits, int misses) : IWarrantyScoreStore
{
    public MemoryWarrantyScoreStore Inner { get; set; } = inner;
    public WarrantyScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyScore? Find(string code)
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

    public void Save(WarrantyScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty scores. Empty fields match all.</summary>
public sealed record WarrantyScoreQuery(string Department, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyScore item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyScore> Apply(List<WarrantyScore> items)
    {
        var matched = Collections.Filter(items, (WarrantyScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty score store.</summary>
public static class WarrantyScoreStore
{
    public static IWarrantyScoreStore NewMemoryWarrantyScoreStore()
    {
        return new MemoryWarrantyScoreStore(new Dictionary<string, WarrantyScore>(), new List<string>());
    }

    public static IWarrantyScoreStore NewCachedWarrantyScoreStore(MemoryWarrantyScoreStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyScoreStore(inner, WarrantyScoreCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<WarrantyScore> FindWarrantyScores(IWarrantyScoreStore store, WarrantyScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
