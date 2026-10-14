namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy schedules by code.</summary>
public interface IPolicyScheduleStore
{
    PolicySchedule? Find(string code);
    void Save(PolicySchedule item);
    bool Remove(string code);
    List<PolicySchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyScheduleStore(Dictionary<string, PolicySchedule> items, List<string> order) : IPolicyScheduleStore
{
    public Dictionary<string, PolicySchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicySchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicySchedule item)
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

    public List<PolicySchedule> List()
    {
        var outValue = new List<PolicySchedule>();
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
public sealed class CachedPolicyScheduleStore(MemoryPolicyScheduleStore inner, PolicyScheduleCache cache, int hits, int misses) : IPolicyScheduleStore
{
    public MemoryPolicyScheduleStore Inner { get; set; } = inner;
    public PolicyScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicySchedule? Find(string code)
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

    public void Save(PolicySchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicySchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy schedules. Empty fields match all.</summary>
public sealed record PolicyScheduleQuery(string Channel, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(PolicySchedule item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicySchedule> Apply(List<PolicySchedule> items)
    {
        var matched = Collections.Filter(items, (PolicySchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy schedule store.</summary>
public static class PolicyScheduleStore
{
    public static IPolicyScheduleStore NewMemoryPolicyScheduleStore()
    {
        return new MemoryPolicyScheduleStore(new Dictionary<string, PolicySchedule>(), new List<string>());
    }

    public static IPolicyScheduleStore NewCachedPolicyScheduleStore(MemoryPolicyScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyScheduleStore(inner, PolicyScheduleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PolicySchedule> FindPolicySchedules(IPolicyScheduleStore store, PolicyScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
