namespace Acme.Workforce.Contractors.Offboarding.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding agreements by code.</summary>
public interface IOffboardingAgreementStore
{
    OffboardingAgreement? Find(string code);
    void Save(OffboardingAgreement item);
    bool Remove(string code);
    List<OffboardingAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingAgreementStore(Dictionary<string, OffboardingAgreement> items, List<string> order) : IOffboardingAgreementStore
{
    public Dictionary<string, OffboardingAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingAgreement item)
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

    public List<OffboardingAgreement> List()
    {
        var outValue = new List<OffboardingAgreement>();
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
public sealed class CachedOffboardingAgreementStore(MemoryOffboardingAgreementStore inner, OffboardingAgreementCache cache, int hits, int misses) : IOffboardingAgreementStore
{
    public MemoryOffboardingAgreementStore Inner { get; set; } = inner;
    public OffboardingAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingAgreement? Find(string code)
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

    public void Save(OffboardingAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding agreements. Empty fields match all.</summary>
public sealed record OffboardingAgreementQuery(string Label, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingAgreement item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OffboardingAgreement> Apply(List<OffboardingAgreement> items)
    {
        var matched = Collections.Filter(items, (OffboardingAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding agreement store.</summary>
public static class OffboardingAgreementStore
{
    public static IOffboardingAgreementStore NewMemoryOffboardingAgreementStore()
    {
        return new MemoryOffboardingAgreementStore(
            new Dictionary<string, OffboardingAgreement>(),
            new List<string>());
    }

    public static IOffboardingAgreementStore NewCachedOffboardingAgreementStore(MemoryOffboardingAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingAgreementStore(inner, OffboardingAgreementCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<OffboardingAgreement> FindOffboardingAgreements(IOffboardingAgreementStore store, OffboardingAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
