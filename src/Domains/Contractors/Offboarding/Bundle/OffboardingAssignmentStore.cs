namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding assignments by code.</summary>
public interface IOffboardingAssignmentStore
{
    OffboardingAssignment? Find(string code);
    void Save(OffboardingAssignment item);
    bool Remove(string code);
    List<OffboardingAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingAssignmentStore(Dictionary<string, OffboardingAssignment> items, List<string> order) : IOffboardingAssignmentStore
{
    public Dictionary<string, OffboardingAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingAssignment item)
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

    public List<OffboardingAssignment> List()
    {
        var outValue = new List<OffboardingAssignment>();
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
public sealed class CachedOffboardingAssignmentStore(MemoryOffboardingAssignmentStore inner, OffboardingAssignmentCache cache, int hits, int misses) : IOffboardingAssignmentStore
{
    public MemoryOffboardingAssignmentStore Inner { get; set; } = inner;
    public OffboardingAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingAssignment? Find(string code)
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

    public void Save(OffboardingAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding assignments. Empty fields match all.</summary>
public sealed record OffboardingAssignmentQuery(string Label, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingAssignment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OffboardingAssignment> Apply(List<OffboardingAssignment> items)
    {
        var matched = Collections.Filter(items, (OffboardingAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding assignment store.</summary>
public static class OffboardingAssignmentStore
{
    public static IOffboardingAssignmentStore NewMemoryOffboardingAssignmentStore()
    {
        return new MemoryOffboardingAssignmentStore(
            new Dictionary<string, OffboardingAssignment>(),
            new List<string>());
    }

    public static IOffboardingAssignmentStore NewCachedOffboardingAssignmentStore(MemoryOffboardingAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingAssignmentStore(inner, OffboardingAssignmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<OffboardingAssignment> FindOffboardingAssignments(IOffboardingAssignmentStore store, OffboardingAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
