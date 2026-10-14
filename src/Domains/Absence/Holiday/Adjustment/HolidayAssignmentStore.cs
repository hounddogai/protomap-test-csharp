namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday assignments by code.</summary>
public interface IHolidayAssignmentStore
{
    HolidayAssignment? Find(string code);
    void Save(HolidayAssignment item);
    bool Remove(string code);
    List<HolidayAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayAssignmentStore(Dictionary<string, HolidayAssignment> items, List<string> order) : IHolidayAssignmentStore
{
    public Dictionary<string, HolidayAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayAssignment item)
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

    public List<HolidayAssignment> List()
    {
        var outValue = new List<HolidayAssignment>();
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
public sealed class CachedHolidayAssignmentStore(MemoryHolidayAssignmentStore inner, HolidayAssignmentCache cache, int hits, int misses) : IHolidayAssignmentStore
{
    public MemoryHolidayAssignmentStore Inner { get; set; } = inner;
    public HolidayAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayAssignment? Find(string code)
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

    public void Save(HolidayAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday assignments. Empty fields match all.</summary>
public sealed record HolidayAssignmentQuery(string Team, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayAssignment item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<HolidayAssignment> Apply(List<HolidayAssignment> items)
    {
        var matched = Collections.Filter(items, (HolidayAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday assignment store.</summary>
public static class HolidayAssignmentStore
{
    public static IHolidayAssignmentStore NewMemoryHolidayAssignmentStore()
    {
        return new MemoryHolidayAssignmentStore(new Dictionary<string, HolidayAssignment>(), new List<string>());
    }

    public static IHolidayAssignmentStore NewCachedHolidayAssignmentStore(MemoryHolidayAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayAssignmentStore(inner, HolidayAssignmentCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<HolidayAssignment> FindHolidayAssignments(IHolidayAssignmentStore store, HolidayAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
