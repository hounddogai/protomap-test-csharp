namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey assignments by code.</summary>
public interface IPulseSurveyAssignmentStore
{
    PulseSurveyAssignment? Find(string code);
    void Save(PulseSurveyAssignment item);
    bool Remove(string code);
    List<PulseSurveyAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyAssignmentStore(Dictionary<string, PulseSurveyAssignment> items, List<string> order) : IPulseSurveyAssignmentStore
{
    public Dictionary<string, PulseSurveyAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyAssignment item)
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

    public List<PulseSurveyAssignment> List()
    {
        var outValue = new List<PulseSurveyAssignment>();
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
public sealed class CachedPulseSurveyAssignmentStore(MemoryPulseSurveyAssignmentStore inner, PulseSurveyAssignmentCache cache, int hits, int misses) : IPulseSurveyAssignmentStore
{
    public MemoryPulseSurveyAssignmentStore Inner { get; set; } = inner;
    public PulseSurveyAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyAssignment? Find(string code)
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

    public void Save(PulseSurveyAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey assignments. Empty fields match all.</summary>
public sealed record PulseSurveyAssignmentQuery(string Country, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyAssignment item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PulseSurveyAssignment> Apply(List<PulseSurveyAssignment> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey assignment store.</summary>
public static class PulseSurveyAssignmentStore
{
    public static IPulseSurveyAssignmentStore NewMemoryPulseSurveyAssignmentStore()
    {
        return new MemoryPulseSurveyAssignmentStore(
            new Dictionary<string, PulseSurveyAssignment>(),
            new List<string>());
    }

    public static IPulseSurveyAssignmentStore NewCachedPulseSurveyAssignmentStore(MemoryPulseSurveyAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyAssignmentStore(inner, PulseSurveyAssignmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PulseSurveyAssignment> FindPulseSurveyAssignments(IPulseSurveyAssignmentStore store, PulseSurveyAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
