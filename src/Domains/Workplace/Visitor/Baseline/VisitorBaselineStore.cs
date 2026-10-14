namespace Acme.Workforce.Workplace.Visitor.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor baselines by code.</summary>
public interface IVisitorBaselineStore
{
    VisitorBaseline? Find(string code);
    void Save(VisitorBaseline item);
    bool Remove(string code);
    List<VisitorBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorBaselineStore(Dictionary<string, VisitorBaseline> items, List<string> order) : IVisitorBaselineStore
{
    public Dictionary<string, VisitorBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorBaseline item)
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

    public List<VisitorBaseline> List()
    {
        var outValue = new List<VisitorBaseline>();
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
public sealed class CachedVisitorBaselineStore(MemoryVisitorBaselineStore inner, VisitorBaselineCache cache, int hits, int misses) : IVisitorBaselineStore
{
    public MemoryVisitorBaselineStore Inner { get; set; } = inner;
    public VisitorBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorBaseline? Find(string code)
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

    public void Save(VisitorBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor baselines. Empty fields match all.</summary>
public sealed record VisitorBaselineQuery(string Title, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorBaseline item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<VisitorBaseline> Apply(List<VisitorBaseline> items)
    {
        var matched = Collections.Filter(items, (VisitorBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor baseline store.</summary>
public static class VisitorBaselineStore
{
    public static IVisitorBaselineStore NewMemoryVisitorBaselineStore()
    {
        return new MemoryVisitorBaselineStore(new Dictionary<string, VisitorBaseline>(), new List<string>());
    }

    public static IVisitorBaselineStore NewCachedVisitorBaselineStore(MemoryVisitorBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorBaselineStore(inner, VisitorBaselineCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<VisitorBaseline> FindVisitorBaselines(IVisitorBaselineStore store, VisitorBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
