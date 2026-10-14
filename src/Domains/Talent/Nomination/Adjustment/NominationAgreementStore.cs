namespace Acme.Workforce.Talent.Nomination.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination agreements by code.</summary>
public interface INominationAgreementStore
{
    NominationAgreement? Find(string code);
    void Save(NominationAgreement item);
    bool Remove(string code);
    List<NominationAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationAgreementStore(Dictionary<string, NominationAgreement> items, List<string> order) : INominationAgreementStore
{
    public Dictionary<string, NominationAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationAgreement item)
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

    public List<NominationAgreement> List()
    {
        var outValue = new List<NominationAgreement>();
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
public sealed class CachedNominationAgreementStore(MemoryNominationAgreementStore inner, NominationAgreementCache cache, int hits, int misses) : INominationAgreementStore
{
    public MemoryNominationAgreementStore Inner { get; set; } = inner;
    public NominationAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationAgreement? Find(string code)
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

    public void Save(NominationAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination agreements. Empty fields match all.</summary>
public sealed record NominationAgreementQuery(string Title, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(NominationAgreement item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<NominationAgreement> Apply(List<NominationAgreement> items)
    {
        var matched = Collections.Filter(items, (NominationAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination agreement store.</summary>
public static class NominationAgreementStore
{
    public static INominationAgreementStore NewMemoryNominationAgreementStore()
    {
        return new MemoryNominationAgreementStore(new Dictionary<string, NominationAgreement>(), new List<string>());
    }

    public static INominationAgreementStore NewCachedNominationAgreementStore(MemoryNominationAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedNominationAgreementStore(inner, NominationAgreementCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<NominationAgreement> FindNominationAgreements(INominationAgreementStore store, NominationAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
