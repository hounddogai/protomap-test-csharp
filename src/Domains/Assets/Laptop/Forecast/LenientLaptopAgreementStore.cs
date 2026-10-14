namespace Acme.Workforce.Assets.Laptop.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient laptop agreements by code.</summary>
public interface ILenientLaptopAgreementStore
{
    LenientLaptopAgreement? Find(string code);
    void Save(LenientLaptopAgreement item);
    bool Remove(string code);
    List<LenientLaptopAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientLaptopAgreementStore(Dictionary<string, LenientLaptopAgreement> items, List<string> order) : ILenientLaptopAgreementStore
{
    public Dictionary<string, LenientLaptopAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientLaptopAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientLaptopAgreement item)
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

    public List<LenientLaptopAgreement> List()
    {
        var outValue = new List<LenientLaptopAgreement>();
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
public sealed class CachedLenientLaptopAgreementStore(MemoryLenientLaptopAgreementStore inner, LenientLaptopAgreementCache cache, int hits, int misses) : ILenientLaptopAgreementStore
{
    public MemoryLenientLaptopAgreementStore Inner { get; set; } = inner;
    public LenientLaptopAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientLaptopAgreement? Find(string code)
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

    public void Save(LenientLaptopAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientLaptopAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient laptop agreements. Empty fields match all.</summary>
public sealed record LenientLaptopAgreementQuery(string Name, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(LenientLaptopAgreement item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientLaptopAgreement> Apply(List<LenientLaptopAgreement> items)
    {
        var matched = Collections.Filter(items, (LenientLaptopAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient laptop agreement store.</summary>
public static class LenientLaptopAgreementStore
{
    public static ILenientLaptopAgreementStore NewMemoryLenientLaptopAgreementStore()
    {
        return new MemoryLenientLaptopAgreementStore(
            new Dictionary<string, LenientLaptopAgreement>(),
            new List<string>());
    }

    public static ILenientLaptopAgreementStore NewCachedLenientLaptopAgreementStore(MemoryLenientLaptopAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedLenientLaptopAgreementStore(
            inner,
            LenientLaptopAgreementCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LenientLaptopAgreement> FindLenientLaptopAgreements(ILenientLaptopAgreementStore store, LenientLaptopAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
