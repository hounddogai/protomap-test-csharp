namespace Acme.Workforce.Engagement.PulseSurvey.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey exceptions by code.</summary>
public interface IPulseSurveyExceptionStore
{
    PulseSurveyException? Find(string code);
    void Save(PulseSurveyException item);
    bool Remove(string code);
    List<PulseSurveyException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyExceptionStore(Dictionary<string, PulseSurveyException> items, List<string> order) : IPulseSurveyExceptionStore
{
    public Dictionary<string, PulseSurveyException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyException item)
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

    public List<PulseSurveyException> List()
    {
        var outValue = new List<PulseSurveyException>();
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
public sealed class CachedPulseSurveyExceptionStore(MemoryPulseSurveyExceptionStore inner, PulseSurveyExceptionCache cache, int hits, int misses) : IPulseSurveyExceptionStore
{
    public MemoryPulseSurveyExceptionStore Inner { get; set; } = inner;
    public PulseSurveyExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyException? Find(string code)
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

    public void Save(PulseSurveyException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey exceptions. Empty fields match all.</summary>
public sealed record PulseSurveyExceptionQuery(string Category, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyException item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PulseSurveyException> Apply(List<PulseSurveyException> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey exception store.</summary>
public static class PulseSurveyExceptionStore
{
    public static IPulseSurveyExceptionStore NewMemoryPulseSurveyExceptionStore()
    {
        return new MemoryPulseSurveyExceptionStore(
            new Dictionary<string, PulseSurveyException>(),
            new List<string>());
    }

    public static IPulseSurveyExceptionStore NewCachedPulseSurveyExceptionStore(MemoryPulseSurveyExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyExceptionStore(inner, PulseSurveyExceptionCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PulseSurveyException> FindPulseSurveyExceptions(IPulseSurveyExceptionStore store, PulseSurveyExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
