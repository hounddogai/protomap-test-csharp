namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary headcount agreements by code.</summary>
public interface IPrimaryHeadcountAgreementStore
{
    PrimaryHeadcountAgreement? Find(string code);
    void Save(PrimaryHeadcountAgreement item);
    bool Remove(string code);
    List<PrimaryHeadcountAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryHeadcountAgreementStore(Dictionary<string, PrimaryHeadcountAgreement> items, List<string> order) : IPrimaryHeadcountAgreementStore
{
    public Dictionary<string, PrimaryHeadcountAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryHeadcountAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryHeadcountAgreement item)
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

    public List<PrimaryHeadcountAgreement> List()
    {
        var outValue = new List<PrimaryHeadcountAgreement>();
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
public sealed class CachedPrimaryHeadcountAgreementStore(MemoryPrimaryHeadcountAgreementStore inner, PrimaryHeadcountAgreementCache cache, int hits, int misses) : IPrimaryHeadcountAgreementStore
{
    public MemoryPrimaryHeadcountAgreementStore Inner { get; set; } = inner;
    public PrimaryHeadcountAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryHeadcountAgreement? Find(string code)
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

    public void Save(PrimaryHeadcountAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryHeadcountAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary headcount agreements. Empty fields match all.</summary>
public sealed record PrimaryHeadcountAgreementQuery(string Title, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryHeadcountAgreement item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryHeadcountAgreement> Apply(List<PrimaryHeadcountAgreement> items)
    {
        var matched = Collections.Filter(items, (PrimaryHeadcountAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary headcount agreement store.</summary>
public static class PrimaryHeadcountAgreementStore
{
    public static IPrimaryHeadcountAgreementStore NewMemoryPrimaryHeadcountAgreementStore()
    {
        return new MemoryPrimaryHeadcountAgreementStore(
            new Dictionary<string, PrimaryHeadcountAgreement>(),
            new List<string>());
    }

    public static IPrimaryHeadcountAgreementStore NewCachedPrimaryHeadcountAgreementStore(MemoryPrimaryHeadcountAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryHeadcountAgreementStore(
            inner,
            PrimaryHeadcountAgreementCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryHeadcountAgreement> FindPrimaryHeadcountAgreements(IPrimaryHeadcountAgreementStore store, PrimaryHeadcountAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
