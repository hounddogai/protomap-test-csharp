namespace Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg requests by code.</summary>
public interface IReorgRequestStore
{
    ReorgRequest? Find(string code);
    void Save(ReorgRequest item);
    bool Remove(string code);
    List<ReorgRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgRequestStore(Dictionary<string, ReorgRequest> items, List<string> order) : IReorgRequestStore
{
    public Dictionary<string, ReorgRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgRequest item)
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

    public List<ReorgRequest> List()
    {
        var outValue = new List<ReorgRequest>();
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
public sealed class CachedReorgRequestStore(MemoryReorgRequestStore inner, ReorgRequestCache cache, int hits, int misses) : IReorgRequestStore
{
    public MemoryReorgRequestStore Inner { get; set; } = inner;
    public ReorgRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgRequest? Find(string code)
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

    public void Save(ReorgRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg requests. Empty fields match all.</summary>
public sealed record ReorgRequestQuery(string Source, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgRequest item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReorgRequest> Apply(List<ReorgRequest> items)
    {
        var matched = Collections.Filter(items, (ReorgRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg request store.</summary>
public static class ReorgRequestStore
{
    public static IReorgRequestStore NewMemoryReorgRequestStore()
    {
        return new MemoryReorgRequestStore(new Dictionary<string, ReorgRequest>(), new List<string>());
    }

    public static IReorgRequestStore NewCachedReorgRequestStore(MemoryReorgRequestStore inner, int ttl, IClock clock)
    {
        return new CachedReorgRequestStore(inner, ReorgRequestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ReorgRequest> FindReorgRequests(IReorgRequestStore store, ReorgRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
