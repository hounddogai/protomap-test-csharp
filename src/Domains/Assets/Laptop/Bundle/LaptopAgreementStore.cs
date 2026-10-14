namespace Acme.Workforce.Assets.Laptop.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop agreements by code.</summary>
public interface ILaptopAgreementStore
{
    LaptopAgreement? Find(string code);
    void Save(LaptopAgreement item);
    bool Remove(string code);
    List<LaptopAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopAgreementStore(Dictionary<string, LaptopAgreement> items, List<string> order) : ILaptopAgreementStore
{
    public Dictionary<string, LaptopAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopAgreement item)
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

    public List<LaptopAgreement> List()
    {
        var outValue = new List<LaptopAgreement>();
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
public sealed class CachedLaptopAgreementStore(MemoryLaptopAgreementStore inner, LaptopAgreementCache cache, int hits, int misses) : ILaptopAgreementStore
{
    public MemoryLaptopAgreementStore Inner { get; set; } = inner;
    public LaptopAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopAgreement? Find(string code)
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

    public void Save(LaptopAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop agreements. Empty fields match all.</summary>
public sealed record LaptopAgreementQuery(string Region, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopAgreement item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopAgreement> Apply(List<LaptopAgreement> items)
    {
        var matched = Collections.Filter(items, (LaptopAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop agreement store.</summary>
public static class LaptopAgreementStore
{
    public static ILaptopAgreementStore NewMemoryLaptopAgreementStore()
    {
        return new MemoryLaptopAgreementStore(new Dictionary<string, LaptopAgreement>(), new List<string>());
    }

    public static ILaptopAgreementStore NewCachedLaptopAgreementStore(MemoryLaptopAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopAgreementStore(inner, LaptopAgreementCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LaptopAgreement> FindLaptopAgreements(ILaptopAgreementStore store, LaptopAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
