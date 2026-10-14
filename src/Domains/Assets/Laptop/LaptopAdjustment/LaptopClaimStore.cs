namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop claims by code.</summary>
public interface ILaptopClaimStore
{
    LaptopClaim? Find(string code);
    void Save(LaptopClaim item);
    bool Remove(string code);
    List<LaptopClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopClaimStore(Dictionary<string, LaptopClaim> items, List<string> order) : ILaptopClaimStore
{
    public Dictionary<string, LaptopClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopClaim item)
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

    public List<LaptopClaim> List()
    {
        var outValue = new List<LaptopClaim>();
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
public sealed class CachedLaptopClaimStore(MemoryLaptopClaimStore inner, LaptopClaimCache cache, int hits, int misses) : ILaptopClaimStore
{
    public MemoryLaptopClaimStore Inner { get; set; } = inner;
    public LaptopClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopClaim? Find(string code)
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

    public void Save(LaptopClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop claims. Empty fields match all.</summary>
public sealed record LaptopClaimQuery(string Name, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopClaim item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<LaptopClaim> Apply(List<LaptopClaim> items)
    {
        var matched = Collections.Filter(items, (LaptopClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop claim store.</summary>
public static class LaptopClaimStore
{
    public static ILaptopClaimStore NewMemoryLaptopClaimStore()
    {
        return new MemoryLaptopClaimStore(new Dictionary<string, LaptopClaim>(), new List<string>());
    }

    public static ILaptopClaimStore NewCachedLaptopClaimStore(MemoryLaptopClaimStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopClaimStore(inner, LaptopClaimCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LaptopClaim> FindLaptopClaims(ILaptopClaimStore store, LaptopClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
