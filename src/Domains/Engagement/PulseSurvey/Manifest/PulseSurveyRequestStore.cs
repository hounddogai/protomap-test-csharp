namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey requests by code.</summary>
public interface IPulseSurveyRequestStore
{
    PulseSurveyRequest? Find(string code);
    void Save(PulseSurveyRequest item);
    bool Remove(string code);
    List<PulseSurveyRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyRequestStore(Dictionary<string, PulseSurveyRequest> items, List<string> order) : IPulseSurveyRequestStore
{
    public Dictionary<string, PulseSurveyRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyRequest item)
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

    public List<PulseSurveyRequest> List()
    {
        var outValue = new List<PulseSurveyRequest>();
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
public sealed class CachedPulseSurveyRequestStore(MemoryPulseSurveyRequestStore inner, PulseSurveyRequestCache cache, int hits, int misses) : IPulseSurveyRequestStore
{
    public MemoryPulseSurveyRequestStore Inner { get; set; } = inner;
    public PulseSurveyRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyRequest? Find(string code)
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

    public void Save(PulseSurveyRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey requests. Empty fields match all.</summary>
public sealed record PulseSurveyRequestQuery(string Department, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyRequest item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PulseSurveyRequest> Apply(List<PulseSurveyRequest> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey request store.</summary>
public static class PulseSurveyRequestStore
{
    public static IPulseSurveyRequestStore NewMemoryPulseSurveyRequestStore()
    {
        return new MemoryPulseSurveyRequestStore(new Dictionary<string, PulseSurveyRequest>(), new List<string>());
    }

    public static IPulseSurveyRequestStore NewCachedPulseSurveyRequestStore(MemoryPulseSurveyRequestStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyRequestStore(inner, PulseSurveyRequestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PulseSurveyRequest> FindPulseSurveyRequests(IPulseSurveyRequestStore store, PulseSurveyRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
