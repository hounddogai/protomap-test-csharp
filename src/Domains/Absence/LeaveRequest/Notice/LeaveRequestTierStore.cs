namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request tiers by code.</summary>
public interface ILeaveRequestTierStore
{
    LeaveRequestTier? Find(string code);
    void Save(LeaveRequestTier item);
    bool Remove(string code);
    List<LeaveRequestTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestTierStore(Dictionary<string, LeaveRequestTier> items, List<string> order) : ILeaveRequestTierStore
{
    public Dictionary<string, LeaveRequestTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestTier item)
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

    public List<LeaveRequestTier> List()
    {
        var outValue = new List<LeaveRequestTier>();
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
public sealed class CachedLeaveRequestTierStore(MemoryLeaveRequestTierStore inner, LeaveRequestTierCache cache, int hits, int misses) : ILeaveRequestTierStore
{
    public MemoryLeaveRequestTierStore Inner { get; set; } = inner;
    public LeaveRequestTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestTier? Find(string code)
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

    public void Save(LeaveRequestTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request tiers. Empty fields match all.</summary>
public sealed record LeaveRequestTierQuery(string Label, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestTier item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LeaveRequestTier> Apply(List<LeaveRequestTier> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request tier store.</summary>
public static class LeaveRequestTierStore
{
    public static ILeaveRequestTierStore NewMemoryLeaveRequestTierStore()
    {
        return new MemoryLeaveRequestTierStore(new Dictionary<string, LeaveRequestTier>(), new List<string>());
    }

    public static ILeaveRequestTierStore NewCachedLeaveRequestTierStore(MemoryLeaveRequestTierStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestTierStore(inner, LeaveRequestTierCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestTier> FindLeaveRequestTiers(ILeaveRequestTierStore store, LeaveRequestTierQuery query)
    {
        return query.Apply(store.List());
    }
}
