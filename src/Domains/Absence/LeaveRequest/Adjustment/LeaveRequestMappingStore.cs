namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request mappings by code.</summary>
public interface ILeaveRequestMappingStore
{
    LeaveRequestMapping? Find(string code);
    void Save(LeaveRequestMapping item);
    bool Remove(string code);
    List<LeaveRequestMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestMappingStore(Dictionary<string, LeaveRequestMapping> items, List<string> order) : ILeaveRequestMappingStore
{
    public Dictionary<string, LeaveRequestMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestMapping item)
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

    public List<LeaveRequestMapping> List()
    {
        var outValue = new List<LeaveRequestMapping>();
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
public sealed class CachedLeaveRequestMappingStore(MemoryLeaveRequestMappingStore inner, LeaveRequestMappingCache cache, int hits, int misses) : ILeaveRequestMappingStore
{
    public MemoryLeaveRequestMappingStore Inner { get; set; } = inner;
    public LeaveRequestMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestMapping? Find(string code)
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

    public void Save(LeaveRequestMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request mappings. Empty fields match all.</summary>
public sealed record LeaveRequestMappingQuery(string Title, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestMapping item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LeaveRequestMapping> Apply(List<LeaveRequestMapping> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request mapping store.</summary>
public static class LeaveRequestMappingStore
{
    public static ILeaveRequestMappingStore NewMemoryLeaveRequestMappingStore()
    {
        return new MemoryLeaveRequestMappingStore(new Dictionary<string, LeaveRequestMapping>(), new List<string>());
    }

    public static ILeaveRequestMappingStore NewCachedLeaveRequestMappingStore(MemoryLeaveRequestMappingStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestMappingStore(inner, LeaveRequestMappingCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestMapping> FindLeaveRequestMappings(ILeaveRequestMappingStore store, LeaveRequestMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
