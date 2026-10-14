namespace Acme.Workforce.Orgdesign.Span.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span profiles by code.</summary>
public interface ISpanProfileStore
{
    SpanProfile? Find(string code);
    void Save(SpanProfile item);
    bool Remove(string code);
    List<SpanProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanProfileStore(Dictionary<string, SpanProfile> items, List<string> order) : ISpanProfileStore
{
    public Dictionary<string, SpanProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanProfile item)
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

    public List<SpanProfile> List()
    {
        var outValue = new List<SpanProfile>();
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
public sealed class CachedSpanProfileStore(MemorySpanProfileStore inner, SpanProfileCache cache, int hits, int misses) : ISpanProfileStore
{
    public MemorySpanProfileStore Inner { get; set; } = inner;
    public SpanProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanProfile? Find(string code)
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

    public void Save(SpanProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span profiles. Empty fields match all.</summary>
public sealed record SpanProfileQuery(string Source, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(SpanProfile item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanProfile> Apply(List<SpanProfile> items)
    {
        var matched = Collections.Filter(items, (SpanProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span profile store.</summary>
public static class SpanProfileStore
{
    public static ISpanProfileStore NewMemorySpanProfileStore()
    {
        return new MemorySpanProfileStore(new Dictionary<string, SpanProfile>(), new List<string>());
    }

    public static ISpanProfileStore NewCachedSpanProfileStore(MemorySpanProfileStore inner, int ttl, IClock clock)
    {
        return new CachedSpanProfileStore(inner, SpanProfileCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SpanProfile> FindSpanProfiles(ISpanProfileStore store, SpanProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
