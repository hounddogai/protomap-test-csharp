namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor claims by code.</summary>
public interface IVisitorClaimStore
{
    VisitorClaim? Find(string code);
    void Save(VisitorClaim item);
    bool Remove(string code);
    List<VisitorClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorClaimStore(Dictionary<string, VisitorClaim> items, List<string> order) : IVisitorClaimStore
{
    public Dictionary<string, VisitorClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorClaim item)
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

    public List<VisitorClaim> List()
    {
        var outValue = new List<VisitorClaim>();
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
public sealed class CachedVisitorClaimStore(MemoryVisitorClaimStore inner, VisitorClaimCache cache, int hits, int misses) : IVisitorClaimStore
{
    public MemoryVisitorClaimStore Inner { get; set; } = inner;
    public VisitorClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorClaim? Find(string code)
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

    public void Save(VisitorClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor claims. Empty fields match all.</summary>
public sealed record VisitorClaimQuery(string Unit, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorClaim item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorClaim> Apply(List<VisitorClaim> items)
    {
        var matched = Collections.Filter(items, (VisitorClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor claim store.</summary>
public static class VisitorClaimStore
{
    public static IVisitorClaimStore NewMemoryVisitorClaimStore()
    {
        return new MemoryVisitorClaimStore(new Dictionary<string, VisitorClaim>(), new List<string>());
    }

    public static IVisitorClaimStore NewCachedVisitorClaimStore(MemoryVisitorClaimStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorClaimStore(inner, VisitorClaimCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<VisitorClaim> FindVisitorClaims(IVisitorClaimStore store, VisitorClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
