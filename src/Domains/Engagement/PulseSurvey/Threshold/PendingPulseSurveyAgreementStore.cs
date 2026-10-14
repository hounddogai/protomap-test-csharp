namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending pulse survey agreements by code.</summary>
public interface IPendingPulseSurveyAgreementStore
{
    PendingPulseSurveyAgreement? Find(string code);
    void Save(PendingPulseSurveyAgreement item);
    bool Remove(string code);
    List<PendingPulseSurveyAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingPulseSurveyAgreementStore(Dictionary<string, PendingPulseSurveyAgreement> items, List<string> order) : IPendingPulseSurveyAgreementStore
{
    public Dictionary<string, PendingPulseSurveyAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingPulseSurveyAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingPulseSurveyAgreement item)
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

    public List<PendingPulseSurveyAgreement> List()
    {
        var outValue = new List<PendingPulseSurveyAgreement>();
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
public sealed class CachedPendingPulseSurveyAgreementStore(MemoryPendingPulseSurveyAgreementStore inner, PendingPulseSurveyAgreementCache cache, int hits, int misses) : IPendingPulseSurveyAgreementStore
{
    public MemoryPendingPulseSurveyAgreementStore Inner { get; set; } = inner;
    public PendingPulseSurveyAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingPulseSurveyAgreement? Find(string code)
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

    public void Save(PendingPulseSurveyAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingPulseSurveyAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending pulse survey agreements. Empty fields match all.</summary>
public sealed record PendingPulseSurveyAgreementQuery(string Team, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(PendingPulseSurveyAgreement item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<PendingPulseSurveyAgreement> Apply(List<PendingPulseSurveyAgreement> items)
    {
        var matched = Collections.Filter(items, (PendingPulseSurveyAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending pulse survey agreement store.</summary>
public static class PendingPulseSurveyAgreementStore
{
    public static IPendingPulseSurveyAgreementStore NewMemoryPendingPulseSurveyAgreementStore()
    {
        return new MemoryPendingPulseSurveyAgreementStore(
            new Dictionary<string, PendingPulseSurveyAgreement>(),
            new List<string>());
    }

    public static IPendingPulseSurveyAgreementStore NewCachedPendingPulseSurveyAgreementStore(MemoryPendingPulseSurveyAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedPendingPulseSurveyAgreementStore(
            inner,
            PendingPulseSurveyAgreementCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PendingPulseSurveyAgreement> FindPendingPulseSurveyAgreements(IPendingPulseSurveyAgreementStore store, PendingPulseSurveyAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
