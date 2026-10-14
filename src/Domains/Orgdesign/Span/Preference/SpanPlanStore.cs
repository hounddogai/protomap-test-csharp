namespace Acme.Workforce.Orgdesign.Span.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span plans by code.</summary>
public interface ISpanPlanStore
{
    SpanPlan? Find(string code);
    void Save(SpanPlan item);
    bool Remove(string code);
    List<SpanPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanPlanStore(Dictionary<string, SpanPlan> items, List<string> order) : ISpanPlanStore
{
    public Dictionary<string, SpanPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanPlan item)
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

    public List<SpanPlan> List()
    {
        var outValue = new List<SpanPlan>();
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
public sealed class CachedSpanPlanStore(MemorySpanPlanStore inner, SpanPlanCache cache, int hits, int misses) : ISpanPlanStore
{
    public MemorySpanPlanStore Inner { get; set; } = inner;
    public SpanPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanPlan? Find(string code)
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

    public void Save(SpanPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span plans. Empty fields match all.</summary>
public sealed record SpanPlanQuery(string Category, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(SpanPlan item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanPlan> Apply(List<SpanPlan> items)
    {
        var matched = Collections.Filter(items, (SpanPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span plan store.</summary>
public static class SpanPlanStore
{
    public static ISpanPlanStore NewMemorySpanPlanStore()
    {
        return new MemorySpanPlanStore(new Dictionary<string, SpanPlan>(), new List<string>());
    }

    public static ISpanPlanStore NewCachedSpanPlanStore(MemorySpanPlanStore inner, int ttl, IClock clock)
    {
        return new CachedSpanPlanStore(inner, SpanPlanCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SpanPlan> FindSpanPlans(ISpanPlanStore store, SpanPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
