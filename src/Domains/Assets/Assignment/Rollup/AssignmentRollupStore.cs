namespace Acme.Workforce.Assets.Assignment.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment rollups by code.</summary>
public interface IAssignmentRollupStore
{
    AssignmentRollup? Find(string code);
    void Save(AssignmentRollup item);
    bool Remove(string code);
    List<AssignmentRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentRollupStore(Dictionary<string, AssignmentRollup> items, List<string> order) : IAssignmentRollupStore
{
    public Dictionary<string, AssignmentRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentRollup item)
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

    public List<AssignmentRollup> List()
    {
        var outValue = new List<AssignmentRollup>();
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
public sealed class CachedAssignmentRollupStore(MemoryAssignmentRollupStore inner, AssignmentRollupCache cache, int hits, int misses) : IAssignmentRollupStore
{
    public MemoryAssignmentRollupStore Inner { get; set; } = inner;
    public AssignmentRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentRollup? Find(string code)
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

    public void Save(AssignmentRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment rollups. Empty fields match all.</summary>
public sealed record AssignmentRollupQuery(string Title, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentRollup item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<AssignmentRollup> Apply(List<AssignmentRollup> items)
    {
        var matched = Collections.Filter(items, (AssignmentRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment rollup store.</summary>
public static class AssignmentRollupStore
{
    public static IAssignmentRollupStore NewMemoryAssignmentRollupStore()
    {
        return new MemoryAssignmentRollupStore(new Dictionary<string, AssignmentRollup>(), new List<string>());
    }

    public static IAssignmentRollupStore NewCachedAssignmentRollupStore(MemoryAssignmentRollupStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentRollupStore(inner, AssignmentRollupCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AssignmentRollup> FindAssignmentRollups(IAssignmentRollupStore store, AssignmentRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
