namespace Acme.Workforce.Orgdesign.Position.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy position assignments by code.</summary>
public interface ILegacyPositionAssignmentStore
{
    LegacyPositionAssignment? Find(string code);
    void Save(LegacyPositionAssignment item);
    bool Remove(string code);
    List<LegacyPositionAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyPositionAssignmentStore(Dictionary<string, LegacyPositionAssignment> items, List<string> order) : ILegacyPositionAssignmentStore
{
    public Dictionary<string, LegacyPositionAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyPositionAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyPositionAssignment item)
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

    public List<LegacyPositionAssignment> List()
    {
        var outValue = new List<LegacyPositionAssignment>();
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
public sealed class CachedLegacyPositionAssignmentStore(MemoryLegacyPositionAssignmentStore inner, LegacyPositionAssignmentCache cache, int hits, int misses) : ILegacyPositionAssignmentStore
{
    public MemoryLegacyPositionAssignmentStore Inner { get; set; } = inner;
    public LegacyPositionAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyPositionAssignment? Find(string code)
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

    public void Save(LegacyPositionAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyPositionAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy position assignments. Empty fields match all.</summary>
public sealed record LegacyPositionAssignmentQuery(string Title, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyPositionAssignment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<LegacyPositionAssignment> Apply(List<LegacyPositionAssignment> items)
    {
        var matched = Collections.Filter(items, (LegacyPositionAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy position assignment store.</summary>
public static class LegacyPositionAssignmentStore
{
    public static ILegacyPositionAssignmentStore NewMemoryLegacyPositionAssignmentStore()
    {
        return new MemoryLegacyPositionAssignmentStore(
            new Dictionary<string, LegacyPositionAssignment>(),
            new List<string>());
    }

    public static ILegacyPositionAssignmentStore NewCachedLegacyPositionAssignmentStore(MemoryLegacyPositionAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyPositionAssignmentStore(
            inner,
            LegacyPositionAssignmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LegacyPositionAssignment> FindLegacyPositionAssignments(ILegacyPositionAssignmentStore store, LegacyPositionAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
