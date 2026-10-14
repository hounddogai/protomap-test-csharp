namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty claims by code.</summary>
public interface IWarrantyClaimStore
{
    WarrantyClaim? Find(string code);
    void Save(WarrantyClaim item);
    bool Remove(string code);
    List<WarrantyClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyClaimStore(Dictionary<string, WarrantyClaim> items, List<string> order) : IWarrantyClaimStore
{
    public Dictionary<string, WarrantyClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyClaim item)
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

    public List<WarrantyClaim> List()
    {
        var outValue = new List<WarrantyClaim>();
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
public sealed class CachedWarrantyClaimStore(MemoryWarrantyClaimStore inner, WarrantyClaimCache cache, int hits, int misses) : IWarrantyClaimStore
{
    public MemoryWarrantyClaimStore Inner { get; set; } = inner;
    public WarrantyClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyClaim? Find(string code)
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

    public void Save(WarrantyClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty claims. Empty fields match all.</summary>
public sealed record WarrantyClaimQuery(string Name, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyClaim item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyClaim> Apply(List<WarrantyClaim> items)
    {
        var matched = Collections.Filter(items, (WarrantyClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty claim store.</summary>
public static class WarrantyClaimStore
{
    public static IWarrantyClaimStore NewMemoryWarrantyClaimStore()
    {
        return new MemoryWarrantyClaimStore(new Dictionary<string, WarrantyClaim>(), new List<string>());
    }

    public static IWarrantyClaimStore NewCachedWarrantyClaimStore(MemoryWarrantyClaimStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyClaimStore(inner, WarrantyClaimCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<WarrantyClaim> FindWarrantyClaims(IWarrantyClaimStore store, WarrantyClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
