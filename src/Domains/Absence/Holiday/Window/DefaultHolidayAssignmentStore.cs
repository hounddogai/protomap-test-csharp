namespace Acme.Workforce.Absence.Holiday.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default holiday assignments by code.</summary>
public interface IDefaultHolidayAssignmentStore
{
    DefaultHolidayAssignment? Find(string code);
    void Save(DefaultHolidayAssignment item);
    bool Remove(string code);
    List<DefaultHolidayAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultHolidayAssignmentStore(Dictionary<string, DefaultHolidayAssignment> items, List<string> order) : IDefaultHolidayAssignmentStore
{
    public Dictionary<string, DefaultHolidayAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultHolidayAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultHolidayAssignment item)
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

    public List<DefaultHolidayAssignment> List()
    {
        var outValue = new List<DefaultHolidayAssignment>();
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
public sealed class CachedDefaultHolidayAssignmentStore(MemoryDefaultHolidayAssignmentStore inner, DefaultHolidayAssignmentCache cache, int hits, int misses) : IDefaultHolidayAssignmentStore
{
    public MemoryDefaultHolidayAssignmentStore Inner { get; set; } = inner;
    public DefaultHolidayAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultHolidayAssignment? Find(string code)
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

    public void Save(DefaultHolidayAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultHolidayAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default holiday assignments. Empty fields match all.</summary>
public sealed record DefaultHolidayAssignmentQuery(string Unit, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultHolidayAssignment item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultHolidayAssignment> Apply(List<DefaultHolidayAssignment> items)
    {
        var matched = Collections.Filter(items, (DefaultHolidayAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default holiday assignment store.</summary>
public static class DefaultHolidayAssignmentStore
{
    public static IDefaultHolidayAssignmentStore NewMemoryDefaultHolidayAssignmentStore()
    {
        return new MemoryDefaultHolidayAssignmentStore(
            new Dictionary<string, DefaultHolidayAssignment>(),
            new List<string>());
    }

    public static IDefaultHolidayAssignmentStore NewCachedDefaultHolidayAssignmentStore(MemoryDefaultHolidayAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultHolidayAssignmentStore(
            inner,
            DefaultHolidayAssignmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<DefaultHolidayAssignment> FindDefaultHolidayAssignments(IDefaultHolidayAssignmentStore store, DefaultHolidayAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
