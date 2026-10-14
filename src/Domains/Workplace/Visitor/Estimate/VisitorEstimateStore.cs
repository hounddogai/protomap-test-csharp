namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor estimates by code.</summary>
public interface IVisitorEstimateStore
{
    VisitorEstimate? Find(string code);
    void Save(VisitorEstimate item);
    bool Remove(string code);
    List<VisitorEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorEstimateStore(Dictionary<string, VisitorEstimate> items, List<string> order) : IVisitorEstimateStore
{
    public Dictionary<string, VisitorEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorEstimate item)
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

    public List<VisitorEstimate> List()
    {
        var outValue = new List<VisitorEstimate>();
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
public sealed class CachedVisitorEstimateStore(MemoryVisitorEstimateStore inner, VisitorEstimateCache cache, int hits, int misses) : IVisitorEstimateStore
{
    public MemoryVisitorEstimateStore Inner { get; set; } = inner;
    public VisitorEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorEstimate? Find(string code)
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

    public void Save(VisitorEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor estimates. Empty fields match all.</summary>
public sealed record VisitorEstimateQuery(string Segment, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorEstimate item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorEstimate> Apply(List<VisitorEstimate> items)
    {
        var matched = Collections.Filter(items, (VisitorEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor estimate store.</summary>
public static class VisitorEstimateStore
{
    public static IVisitorEstimateStore NewMemoryVisitorEstimateStore()
    {
        return new MemoryVisitorEstimateStore(new Dictionary<string, VisitorEstimate>(), new List<string>());
    }

    public static IVisitorEstimateStore NewCachedVisitorEstimateStore(MemoryVisitorEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorEstimateStore(inner, VisitorEstimateCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<VisitorEstimate> FindVisitorEstimates(IVisitorEstimateStore store, VisitorEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
