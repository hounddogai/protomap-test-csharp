namespace Acme.Workforce.Assets.Warranty.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional warranty exceptions by code.</summary>
public interface IRegionalWarrantyExceptionStore
{
    RegionalWarrantyException? Find(string code);
    void Save(RegionalWarrantyException item);
    bool Remove(string code);
    List<RegionalWarrantyException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalWarrantyExceptionStore(Dictionary<string, RegionalWarrantyException> items, List<string> order) : IRegionalWarrantyExceptionStore
{
    public Dictionary<string, RegionalWarrantyException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalWarrantyException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalWarrantyException item)
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

    public List<RegionalWarrantyException> List()
    {
        var outValue = new List<RegionalWarrantyException>();
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
public sealed class CachedRegionalWarrantyExceptionStore(MemoryRegionalWarrantyExceptionStore inner, RegionalWarrantyExceptionCache cache, int hits, int misses) : IRegionalWarrantyExceptionStore
{
    public MemoryRegionalWarrantyExceptionStore Inner { get; set; } = inner;
    public RegionalWarrantyExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalWarrantyException? Find(string code)
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

    public void Save(RegionalWarrantyException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalWarrantyException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional warranty exceptions. Empty fields match all.</summary>
public sealed record RegionalWarrantyExceptionQuery(string Name, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalWarrantyException item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalWarrantyException> Apply(List<RegionalWarrantyException> items)
    {
        var matched = Collections.Filter(items, (RegionalWarrantyException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional warranty exception store.</summary>
public static class RegionalWarrantyExceptionStore
{
    public static IRegionalWarrantyExceptionStore NewMemoryRegionalWarrantyExceptionStore()
    {
        return new MemoryRegionalWarrantyExceptionStore(
            new Dictionary<string, RegionalWarrantyException>(),
            new List<string>());
    }

    public static IRegionalWarrantyExceptionStore NewCachedRegionalWarrantyExceptionStore(MemoryRegionalWarrantyExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalWarrantyExceptionStore(
            inner,
            RegionalWarrantyExceptionCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RegionalWarrantyException> FindRegionalWarrantyExceptions(IRegionalWarrantyExceptionStore store, RegionalWarrantyExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
