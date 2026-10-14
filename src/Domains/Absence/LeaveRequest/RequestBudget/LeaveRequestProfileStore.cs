namespace Acme.Workforce.Absence.LeaveRequest.RequestBudget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request profiles by code.</summary>
public interface ILeaveRequestProfileStore
{
    LeaveRequestProfile? Find(string code);
    void Save(LeaveRequestProfile item);
    bool Remove(string code);
    List<LeaveRequestProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestProfileStore(Dictionary<string, LeaveRequestProfile> items, List<string> order) : ILeaveRequestProfileStore
{
    public Dictionary<string, LeaveRequestProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestProfile item)
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

    public List<LeaveRequestProfile> List()
    {
        var outValue = new List<LeaveRequestProfile>();
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
public sealed class CachedLeaveRequestProfileStore(MemoryLeaveRequestProfileStore inner, LeaveRequestProfileCache cache, int hits, int misses) : ILeaveRequestProfileStore
{
    public MemoryLeaveRequestProfileStore Inner { get; set; } = inner;
    public LeaveRequestProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestProfile? Find(string code)
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

    public void Save(LeaveRequestProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request profiles. Empty fields match all.</summary>
public sealed record LeaveRequestProfileQuery(string Country, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestProfile item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LeaveRequestProfile> Apply(List<LeaveRequestProfile> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request profile store.</summary>
public static class LeaveRequestProfileStore
{
    public static ILeaveRequestProfileStore NewMemoryLeaveRequestProfileStore()
    {
        return new MemoryLeaveRequestProfileStore(new Dictionary<string, LeaveRequestProfile>(), new List<string>());
    }

    public static ILeaveRequestProfileStore NewCachedLeaveRequestProfileStore(MemoryLeaveRequestProfileStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestProfileStore(inner, LeaveRequestProfileCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestProfile> FindLeaveRequestProfiles(ILeaveRequestProfileStore store, LeaveRequestProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
