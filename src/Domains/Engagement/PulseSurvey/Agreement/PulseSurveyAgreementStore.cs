namespace Acme.Workforce.Engagement.PulseSurvey.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey agreements by code.</summary>
public interface IPulseSurveyAgreementStore
{
    PulseSurveyAgreement? Find(string code);
    void Save(PulseSurveyAgreement item);
    bool Remove(string code);
    List<PulseSurveyAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyAgreementStore(Dictionary<string, PulseSurveyAgreement> items, List<string> order) : IPulseSurveyAgreementStore
{
    public Dictionary<string, PulseSurveyAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyAgreement item)
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

    public List<PulseSurveyAgreement> List()
    {
        var outValue = new List<PulseSurveyAgreement>();
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
public sealed class CachedPulseSurveyAgreementStore(MemoryPulseSurveyAgreementStore inner, PulseSurveyAgreementCache cache, int hits, int misses) : IPulseSurveyAgreementStore
{
    public MemoryPulseSurveyAgreementStore Inner { get; set; } = inner;
    public PulseSurveyAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyAgreement? Find(string code)
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

    public void Save(PulseSurveyAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey agreements. Empty fields match all.</summary>
public sealed record PulseSurveyAgreementQuery(string Category, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyAgreement item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PulseSurveyAgreement> Apply(List<PulseSurveyAgreement> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey agreement store.</summary>
public static class PulseSurveyAgreementStore
{
    public static IPulseSurveyAgreementStore NewMemoryPulseSurveyAgreementStore()
    {
        return new MemoryPulseSurveyAgreementStore(
            new Dictionary<string, PulseSurveyAgreement>(),
            new List<string>());
    }

    public static IPulseSurveyAgreementStore NewCachedPulseSurveyAgreementStore(MemoryPulseSurveyAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyAgreementStore(inner, PulseSurveyAgreementCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PulseSurveyAgreement> FindPulseSurveyAgreements(IPulseSurveyAgreementStore store, PulseSurveyAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
