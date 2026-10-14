namespace Acme.Workforce.Orgdesign.Span.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span bundles by code.</summary>
public interface ISpanBundleStore
{
    SpanBundle? Find(string code);
    void Save(SpanBundle item);
    bool Remove(string code);
    List<SpanBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanBundleStore(Dictionary<string, SpanBundle> items, List<string> order) : ISpanBundleStore
{
    public Dictionary<string, SpanBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanBundle item)
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

    public List<SpanBundle> List()
    {
        var outValue = new List<SpanBundle>();
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
public sealed class CachedSpanBundleStore(MemorySpanBundleStore inner, SpanBundleCache cache, int hits, int misses) : ISpanBundleStore
{
    public MemorySpanBundleStore Inner { get; set; } = inner;
    public SpanBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanBundle? Find(string code)
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

    public void Save(SpanBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span bundles. Empty fields match all.</summary>
public sealed record SpanBundleQuery(string Name, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(SpanBundle item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanBundle> Apply(List<SpanBundle> items)
    {
        var matched = Collections.Filter(items, (SpanBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span bundle store.</summary>
public static class SpanBundleStore
{
    public static ISpanBundleStore NewMemorySpanBundleStore()
    {
        return new MemorySpanBundleStore(new Dictionary<string, SpanBundle>(), new List<string>());
    }

    public static ISpanBundleStore NewCachedSpanBundleStore(MemorySpanBundleStore inner, int ttl, IClock clock)
    {
        return new CachedSpanBundleStore(inner, SpanBundleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<SpanBundle> FindSpanBundles(ISpanBundleStore store, SpanBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
