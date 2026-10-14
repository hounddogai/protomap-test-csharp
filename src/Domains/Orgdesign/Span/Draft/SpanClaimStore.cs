namespace Acme.Workforce.Orgdesign.Span.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span claims by code.</summary>
public interface ISpanClaimStore
{
    SpanClaim? Find(string code);
    void Save(SpanClaim item);
    bool Remove(string code);
    List<SpanClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanClaimStore(Dictionary<string, SpanClaim> items, List<string> order) : ISpanClaimStore
{
    public Dictionary<string, SpanClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanClaim item)
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

    public List<SpanClaim> List()
    {
        var outValue = new List<SpanClaim>();
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
public sealed class CachedSpanClaimStore(MemorySpanClaimStore inner, SpanClaimCache cache, int hits, int misses) : ISpanClaimStore
{
    public MemorySpanClaimStore Inner { get; set; } = inner;
    public SpanClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanClaim? Find(string code)
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

    public void Save(SpanClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span claims. Empty fields match all.</summary>
public sealed record SpanClaimQuery(string Country, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(SpanClaim item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanClaim> Apply(List<SpanClaim> items)
    {
        var matched = Collections.Filter(items, (SpanClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span claim store.</summary>
public static class SpanClaimStore
{
    public static ISpanClaimStore NewMemorySpanClaimStore()
    {
        return new MemorySpanClaimStore(new Dictionary<string, SpanClaim>(), new List<string>());
    }

    public static ISpanClaimStore NewCachedSpanClaimStore(MemorySpanClaimStore inner, int ttl, IClock clock)
    {
        return new CachedSpanClaimStore(inner, SpanClaimCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<SpanClaim> FindSpanClaims(ISpanClaimStore store, SpanClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
