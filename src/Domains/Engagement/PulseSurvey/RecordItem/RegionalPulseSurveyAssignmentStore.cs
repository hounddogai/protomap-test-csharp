namespace Acme.Workforce.Engagement.PulseSurvey.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional pulse survey assignments by code.</summary>
public interface IRegionalPulseSurveyAssignmentStore
{
    RegionalPulseSurveyAssignment? Find(string code);
    void Save(RegionalPulseSurveyAssignment item);
    bool Remove(string code);
    List<RegionalPulseSurveyAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalPulseSurveyAssignmentStore(Dictionary<string, RegionalPulseSurveyAssignment> items, List<string> order) : IRegionalPulseSurveyAssignmentStore
{
    public Dictionary<string, RegionalPulseSurveyAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalPulseSurveyAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalPulseSurveyAssignment item)
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

    public List<RegionalPulseSurveyAssignment> List()
    {
        var outValue = new List<RegionalPulseSurveyAssignment>();
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
public sealed class CachedRegionalPulseSurveyAssignmentStore(MemoryRegionalPulseSurveyAssignmentStore inner, RegionalPulseSurveyAssignmentCache cache, int hits, int misses) : IRegionalPulseSurveyAssignmentStore
{
    public MemoryRegionalPulseSurveyAssignmentStore Inner { get; set; } = inner;
    public RegionalPulseSurveyAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalPulseSurveyAssignment? Find(string code)
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

    public void Save(RegionalPulseSurveyAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalPulseSurveyAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional pulse survey assignments. Empty fields match all.</summary>
public sealed record RegionalPulseSurveyAssignmentQuery(string Label, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalPulseSurveyAssignment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalPulseSurveyAssignment> Apply(List<RegionalPulseSurveyAssignment> items)
    {
        var matched = Collections.Filter(items, (RegionalPulseSurveyAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional pulse survey assignment store.</summary>
public static class RegionalPulseSurveyAssignmentStore
{
    public static IRegionalPulseSurveyAssignmentStore NewMemoryRegionalPulseSurveyAssignmentStore()
    {
        return new MemoryRegionalPulseSurveyAssignmentStore(
            new Dictionary<string, RegionalPulseSurveyAssignment>(),
            new List<string>());
    }

    public static IRegionalPulseSurveyAssignmentStore NewCachedRegionalPulseSurveyAssignmentStore(MemoryRegionalPulseSurveyAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalPulseSurveyAssignmentStore(
            inner,
            RegionalPulseSurveyAssignmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RegionalPulseSurveyAssignment> FindRegionalPulseSurveyAssignments(IRegionalPulseSurveyAssignmentStore store, RegionalPulseSurveyAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
