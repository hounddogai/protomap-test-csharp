namespace Acme.Workforce.Talent.CareerPath.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path claims by code.</summary>
public interface ICareerPathClaimStore
{
    CareerPathClaim? Find(string code);
    void Save(CareerPathClaim item);
    bool Remove(string code);
    List<CareerPathClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathClaimStore(Dictionary<string, CareerPathClaim> items, List<string> order) : ICareerPathClaimStore
{
    public Dictionary<string, CareerPathClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathClaim item)
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

    public List<CareerPathClaim> List()
    {
        var outValue = new List<CareerPathClaim>();
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
public sealed class CachedCareerPathClaimStore(MemoryCareerPathClaimStore inner, CareerPathClaimCache cache, int hits, int misses) : ICareerPathClaimStore
{
    public MemoryCareerPathClaimStore Inner { get; set; } = inner;
    public CareerPathClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathClaim? Find(string code)
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

    public void Save(CareerPathClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path claims. Empty fields match all.</summary>
public sealed record CareerPathClaimQuery(string Unit, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathClaim item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathClaim> Apply(List<CareerPathClaim> items)
    {
        var matched = Collections.Filter(items, (CareerPathClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path claim store.</summary>
public static class CareerPathClaimStore
{
    public static ICareerPathClaimStore NewMemoryCareerPathClaimStore()
    {
        return new MemoryCareerPathClaimStore(new Dictionary<string, CareerPathClaim>(), new List<string>());
    }

    public static ICareerPathClaimStore NewCachedCareerPathClaimStore(MemoryCareerPathClaimStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathClaimStore(inner, CareerPathClaimCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<CareerPathClaim> FindCareerPathClaims(ICareerPathClaimStore store, CareerPathClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
