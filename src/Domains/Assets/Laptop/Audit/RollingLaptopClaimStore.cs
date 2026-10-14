namespace Acme.Workforce.Assets.Laptop.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling laptop claims by code.</summary>
public interface IRollingLaptopClaimStore
{
    RollingLaptopClaim? Find(string code);
    void Save(RollingLaptopClaim item);
    bool Remove(string code);
    List<RollingLaptopClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingLaptopClaimStore(Dictionary<string, RollingLaptopClaim> items, List<string> order) : IRollingLaptopClaimStore
{
    public Dictionary<string, RollingLaptopClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingLaptopClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingLaptopClaim item)
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

    public List<RollingLaptopClaim> List()
    {
        var outValue = new List<RollingLaptopClaim>();
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
public sealed class CachedRollingLaptopClaimStore(MemoryRollingLaptopClaimStore inner, RollingLaptopClaimCache cache, int hits, int misses) : IRollingLaptopClaimStore
{
    public MemoryRollingLaptopClaimStore Inner { get; set; } = inner;
    public RollingLaptopClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingLaptopClaim? Find(string code)
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

    public void Save(RollingLaptopClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingLaptopClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling laptop claims. Empty fields match all.</summary>
public sealed record RollingLaptopClaimQuery(string Name, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(RollingLaptopClaim item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingLaptopClaim> Apply(List<RollingLaptopClaim> items)
    {
        var matched = Collections.Filter(items, (RollingLaptopClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling laptop claim store.</summary>
public static class RollingLaptopClaimStore
{
    public static IRollingLaptopClaimStore NewMemoryRollingLaptopClaimStore()
    {
        return new MemoryRollingLaptopClaimStore(new Dictionary<string, RollingLaptopClaim>(), new List<string>());
    }

    public static IRollingLaptopClaimStore NewCachedRollingLaptopClaimStore(MemoryRollingLaptopClaimStore inner, int ttl, IClock clock)
    {
        return new CachedRollingLaptopClaimStore(inner, RollingLaptopClaimCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RollingLaptopClaim> FindRollingLaptopClaims(IRollingLaptopClaimStore store, RollingLaptopClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
