namespace Acme.Workforce.Orgdesign.Span.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span quotas by code.</summary>
public interface ISpanQuotaStore
{
    SpanQuota? Find(string code);
    void Save(SpanQuota item);
    bool Remove(string code);
    List<SpanQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanQuotaStore(Dictionary<string, SpanQuota> items, List<string> order) : ISpanQuotaStore
{
    public Dictionary<string, SpanQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanQuota item)
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

    public List<SpanQuota> List()
    {
        var outValue = new List<SpanQuota>();
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
public sealed class CachedSpanQuotaStore(MemorySpanQuotaStore inner, SpanQuotaCache cache, int hits, int misses) : ISpanQuotaStore
{
    public MemorySpanQuotaStore Inner { get; set; } = inner;
    public SpanQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanQuota? Find(string code)
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

    public void Save(SpanQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span quotas. Empty fields match all.</summary>
public sealed record SpanQuotaQuery(string Label, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(SpanQuota item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanQuota> Apply(List<SpanQuota> items)
    {
        var matched = Collections.Filter(items, (SpanQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span quota store.</summary>
public static class SpanQuotaStore
{
    public static ISpanQuotaStore NewMemorySpanQuotaStore()
    {
        return new MemorySpanQuotaStore(new Dictionary<string, SpanQuota>(), new List<string>());
    }

    public static ISpanQuotaStore NewCachedSpanQuotaStore(MemorySpanQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedSpanQuotaStore(inner, SpanQuotaCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SpanQuota> FindSpanQuotas(ISpanQuotaStore store, SpanQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
