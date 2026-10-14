namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk claims by code.</summary>
public interface IDeskClaimStore
{
    DeskClaim? Find(string code);
    void Save(DeskClaim item);
    bool Remove(string code);
    List<DeskClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskClaimStore(Dictionary<string, DeskClaim> items, List<string> order) : IDeskClaimStore
{
    public Dictionary<string, DeskClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskClaim item)
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

    public List<DeskClaim> List()
    {
        var outValue = new List<DeskClaim>();
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
public sealed class CachedDeskClaimStore(MemoryDeskClaimStore inner, DeskClaimCache cache, int hits, int misses) : IDeskClaimStore
{
    public MemoryDeskClaimStore Inner { get; set; } = inner;
    public DeskClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskClaim? Find(string code)
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

    public void Save(DeskClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk claims. Empty fields match all.</summary>
public sealed record DeskClaimQuery(string Department, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(DeskClaim item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<DeskClaim> Apply(List<DeskClaim> items)
    {
        var matched = Collections.Filter(items, (DeskClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk claim store.</summary>
public static class DeskClaimStore
{
    public static IDeskClaimStore NewMemoryDeskClaimStore()
    {
        return new MemoryDeskClaimStore(new Dictionary<string, DeskClaim>(), new List<string>());
    }

    public static IDeskClaimStore NewCachedDeskClaimStore(MemoryDeskClaimStore inner, int ttl, IClock clock)
    {
        return new CachedDeskClaimStore(inner, DeskClaimCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<DeskClaim> FindDeskClaims(IDeskClaimStore store, DeskClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
