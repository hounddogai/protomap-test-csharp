namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge agreements by code.</summary>
public interface IAccessBadgeAgreementStore
{
    AccessBadgeAgreement? Find(string code);
    void Save(AccessBadgeAgreement item);
    bool Remove(string code);
    List<AccessBadgeAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeAgreementStore(Dictionary<string, AccessBadgeAgreement> items, List<string> order) : IAccessBadgeAgreementStore
{
    public Dictionary<string, AccessBadgeAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeAgreement item)
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

    public List<AccessBadgeAgreement> List()
    {
        var outValue = new List<AccessBadgeAgreement>();
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
public sealed class CachedAccessBadgeAgreementStore(MemoryAccessBadgeAgreementStore inner, AccessBadgeAgreementCache cache, int hits, int misses) : IAccessBadgeAgreementStore
{
    public MemoryAccessBadgeAgreementStore Inner { get; set; } = inner;
    public AccessBadgeAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeAgreement? Find(string code)
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

    public void Save(AccessBadgeAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge agreements. Empty fields match all.</summary>
public sealed record AccessBadgeAgreementQuery(string Name, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeAgreement item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccessBadgeAgreement> Apply(List<AccessBadgeAgreement> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge agreement store.</summary>
public static class AccessBadgeAgreementStore
{
    public static IAccessBadgeAgreementStore NewMemoryAccessBadgeAgreementStore()
    {
        return new MemoryAccessBadgeAgreementStore(
            new Dictionary<string, AccessBadgeAgreement>(),
            new List<string>());
    }

    public static IAccessBadgeAgreementStore NewCachedAccessBadgeAgreementStore(MemoryAccessBadgeAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeAgreementStore(inner, AccessBadgeAgreementCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeAgreement> FindAccessBadgeAgreements(IAccessBadgeAgreementStore store, AccessBadgeAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
