namespace Acme.Workforce.Orgdesign.Headcount.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount agreements by code.</summary>
public interface IHeadcountAgreementStore
{
    HeadcountAgreement? Find(string code);
    void Save(HeadcountAgreement item);
    bool Remove(string code);
    List<HeadcountAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountAgreementStore(Dictionary<string, HeadcountAgreement> items, List<string> order) : IHeadcountAgreementStore
{
    public Dictionary<string, HeadcountAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountAgreement item)
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

    public List<HeadcountAgreement> List()
    {
        var outValue = new List<HeadcountAgreement>();
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
public sealed class CachedHeadcountAgreementStore(MemoryHeadcountAgreementStore inner, HeadcountAgreementCache cache, int hits, int misses) : IHeadcountAgreementStore
{
    public MemoryHeadcountAgreementStore Inner { get; set; } = inner;
    public HeadcountAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountAgreement? Find(string code)
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

    public void Save(HeadcountAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount agreements. Empty fields match all.</summary>
public sealed record HeadcountAgreementQuery(string Label, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountAgreement item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountAgreement> Apply(List<HeadcountAgreement> items)
    {
        var matched = Collections.Filter(items, (HeadcountAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount agreement store.</summary>
public static class HeadcountAgreementStore
{
    public static IHeadcountAgreementStore NewMemoryHeadcountAgreementStore()
    {
        return new MemoryHeadcountAgreementStore(new Dictionary<string, HeadcountAgreement>(), new List<string>());
    }

    public static IHeadcountAgreementStore NewCachedHeadcountAgreementStore(MemoryHeadcountAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountAgreementStore(inner, HeadcountAgreementCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<HeadcountAgreement> FindHeadcountAgreements(IHeadcountAgreementStore store, HeadcountAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
