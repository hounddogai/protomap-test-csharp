namespace Acme.Workforce.Orgdesign.Span.SpanDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default span claims by code.</summary>
public interface IDefaultSpanClaimStore
{
    DefaultSpanClaim? Find(string code);
    void Save(DefaultSpanClaim item);
    bool Remove(string code);
    List<DefaultSpanClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultSpanClaimStore(Dictionary<string, DefaultSpanClaim> items, List<string> order) : IDefaultSpanClaimStore
{
    public Dictionary<string, DefaultSpanClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultSpanClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultSpanClaim item)
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

    public List<DefaultSpanClaim> List()
    {
        var outValue = new List<DefaultSpanClaim>();
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
public sealed class CachedDefaultSpanClaimStore(MemoryDefaultSpanClaimStore inner, DefaultSpanClaimCache cache, int hits, int misses) : IDefaultSpanClaimStore
{
    public MemoryDefaultSpanClaimStore Inner { get; set; } = inner;
    public DefaultSpanClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultSpanClaim? Find(string code)
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

    public void Save(DefaultSpanClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultSpanClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default span claims. Empty fields match all.</summary>
public sealed record DefaultSpanClaimQuery(string Unit, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultSpanClaim item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultSpanClaim> Apply(List<DefaultSpanClaim> items)
    {
        var matched = Collections.Filter(items, (DefaultSpanClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default span claim store.</summary>
public static class DefaultSpanClaimStore
{
    public static IDefaultSpanClaimStore NewMemoryDefaultSpanClaimStore()
    {
        return new MemoryDefaultSpanClaimStore(new Dictionary<string, DefaultSpanClaim>(), new List<string>());
    }

    public static IDefaultSpanClaimStore NewCachedDefaultSpanClaimStore(MemoryDefaultSpanClaimStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultSpanClaimStore(inner, DefaultSpanClaimCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DefaultSpanClaim> FindDefaultSpanClaims(IDefaultSpanClaimStore store, DefaultSpanClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
