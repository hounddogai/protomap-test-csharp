namespace Acme.Workforce.Assets.Warranty.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty agreements by code.</summary>
public interface IWarrantyAgreementStore
{
    WarrantyAgreement? Find(string code);
    void Save(WarrantyAgreement item);
    bool Remove(string code);
    List<WarrantyAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyAgreementStore(Dictionary<string, WarrantyAgreement> items, List<string> order) : IWarrantyAgreementStore
{
    public Dictionary<string, WarrantyAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyAgreement item)
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

    public List<WarrantyAgreement> List()
    {
        var outValue = new List<WarrantyAgreement>();
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
public sealed class CachedWarrantyAgreementStore(MemoryWarrantyAgreementStore inner, WarrantyAgreementCache cache, int hits, int misses) : IWarrantyAgreementStore
{
    public MemoryWarrantyAgreementStore Inner { get; set; } = inner;
    public WarrantyAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyAgreement? Find(string code)
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

    public void Save(WarrantyAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty agreements. Empty fields match all.</summary>
public sealed record WarrantyAgreementQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyAgreement item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyAgreement> Apply(List<WarrantyAgreement> items)
    {
        var matched = Collections.Filter(items, (WarrantyAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty agreement store.</summary>
public static class WarrantyAgreementStore
{
    public static IWarrantyAgreementStore NewMemoryWarrantyAgreementStore()
    {
        return new MemoryWarrantyAgreementStore(new Dictionary<string, WarrantyAgreement>(), new List<string>());
    }

    public static IWarrantyAgreementStore NewCachedWarrantyAgreementStore(MemoryWarrantyAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyAgreementStore(inner, WarrantyAgreementCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<WarrantyAgreement> FindWarrantyAgreements(IWarrantyAgreementStore store, WarrantyAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
