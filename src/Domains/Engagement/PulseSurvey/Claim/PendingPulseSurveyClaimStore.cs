namespace Acme.Workforce.Engagement.PulseSurvey.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending pulse survey claims by code.</summary>
public interface IPendingPulseSurveyClaimStore
{
    PendingPulseSurveyClaim? Find(string code);
    void Save(PendingPulseSurveyClaim item);
    bool Remove(string code);
    List<PendingPulseSurveyClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingPulseSurveyClaimStore(Dictionary<string, PendingPulseSurveyClaim> items, List<string> order) : IPendingPulseSurveyClaimStore
{
    public Dictionary<string, PendingPulseSurveyClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingPulseSurveyClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingPulseSurveyClaim item)
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

    public List<PendingPulseSurveyClaim> List()
    {
        var outValue = new List<PendingPulseSurveyClaim>();
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
public sealed class CachedPendingPulseSurveyClaimStore(MemoryPendingPulseSurveyClaimStore inner, PendingPulseSurveyClaimCache cache, int hits, int misses) : IPendingPulseSurveyClaimStore
{
    public MemoryPendingPulseSurveyClaimStore Inner { get; set; } = inner;
    public PendingPulseSurveyClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingPulseSurveyClaim? Find(string code)
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

    public void Save(PendingPulseSurveyClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingPulseSurveyClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending pulse survey claims. Empty fields match all.</summary>
public sealed record PendingPulseSurveyClaimQuery(string Department, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(PendingPulseSurveyClaim item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingPulseSurveyClaim> Apply(List<PendingPulseSurveyClaim> items)
    {
        var matched = Collections.Filter(items, (PendingPulseSurveyClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending pulse survey claim store.</summary>
public static class PendingPulseSurveyClaimStore
{
    public static IPendingPulseSurveyClaimStore NewMemoryPendingPulseSurveyClaimStore()
    {
        return new MemoryPendingPulseSurveyClaimStore(
            new Dictionary<string, PendingPulseSurveyClaim>(),
            new List<string>());
    }

    public static IPendingPulseSurveyClaimStore NewCachedPendingPulseSurveyClaimStore(MemoryPendingPulseSurveyClaimStore inner, int ttl, IClock clock)
    {
        return new CachedPendingPulseSurveyClaimStore(
            inner,
            PendingPulseSurveyClaimCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PendingPulseSurveyClaim> FindPendingPulseSurveyClaims(IPendingPulseSurveyClaimStore store, PendingPulseSurveyClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
