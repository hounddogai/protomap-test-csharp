namespace Acme.Workforce.Assets.Warranty.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty exceptions by code.</summary>
public interface IWarrantyExceptionStore
{
    WarrantyException? Find(string code);
    void Save(WarrantyException item);
    bool Remove(string code);
    List<WarrantyException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyExceptionStore(Dictionary<string, WarrantyException> items, List<string> order) : IWarrantyExceptionStore
{
    public Dictionary<string, WarrantyException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyException item)
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

    public List<WarrantyException> List()
    {
        var outValue = new List<WarrantyException>();
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
public sealed class CachedWarrantyExceptionStore(MemoryWarrantyExceptionStore inner, WarrantyExceptionCache cache, int hits, int misses) : IWarrantyExceptionStore
{
    public MemoryWarrantyExceptionStore Inner { get; set; } = inner;
    public WarrantyExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyException? Find(string code)
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

    public void Save(WarrantyException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty exceptions. Empty fields match all.</summary>
public sealed record WarrantyExceptionQuery(string Region, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyException item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<WarrantyException> Apply(List<WarrantyException> items)
    {
        var matched = Collections.Filter(items, (WarrantyException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty exception store.</summary>
public static class WarrantyExceptionStore
{
    public static IWarrantyExceptionStore NewMemoryWarrantyExceptionStore()
    {
        return new MemoryWarrantyExceptionStore(new Dictionary<string, WarrantyException>(), new List<string>());
    }

    public static IWarrantyExceptionStore NewCachedWarrantyExceptionStore(MemoryWarrantyExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyExceptionStore(inner, WarrantyExceptionCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<WarrantyException> FindWarrantyExceptions(IWarrantyExceptionStore store, WarrantyExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
