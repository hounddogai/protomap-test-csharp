namespace Acme.Workforce.Assets.Assignment.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment summaries by code.</summary>
public interface IAssignmentSummaryStore
{
    AssignmentSummary? Find(string code);
    void Save(AssignmentSummary item);
    bool Remove(string code);
    List<AssignmentSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentSummaryStore(Dictionary<string, AssignmentSummary> items, List<string> order) : IAssignmentSummaryStore
{
    public Dictionary<string, AssignmentSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentSummary item)
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

    public List<AssignmentSummary> List()
    {
        var outValue = new List<AssignmentSummary>();
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
public sealed class CachedAssignmentSummaryStore(MemoryAssignmentSummaryStore inner, AssignmentSummaryCache cache, int hits, int misses) : IAssignmentSummaryStore
{
    public MemoryAssignmentSummaryStore Inner { get; set; } = inner;
    public AssignmentSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentSummary? Find(string code)
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

    public void Save(AssignmentSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment summaries. Empty fields match all.</summary>
public sealed record AssignmentSummaryQuery(string Source, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentSummary item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<AssignmentSummary> Apply(List<AssignmentSummary> items)
    {
        var matched = Collections.Filter(items, (AssignmentSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment summary store.</summary>
public static class AssignmentSummaryStore
{
    public static IAssignmentSummaryStore NewMemoryAssignmentSummaryStore()
    {
        return new MemoryAssignmentSummaryStore(new Dictionary<string, AssignmentSummary>(), new List<string>());
    }

    public static IAssignmentSummaryStore NewCachedAssignmentSummaryStore(MemoryAssignmentSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentSummaryStore(inner, AssignmentSummaryCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AssignmentSummary> FindAssignmentSummaries(IAssignmentSummaryStore store, AssignmentSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
