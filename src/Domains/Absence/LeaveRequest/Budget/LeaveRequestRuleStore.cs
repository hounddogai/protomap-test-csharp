namespace Acme.Workforce.Absence.LeaveRequest.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request rules by code.</summary>
public interface ILeaveRequestRuleStore
{
    LeaveRequestRule? Find(string code);
    void Save(LeaveRequestRule item);
    bool Remove(string code);
    List<LeaveRequestRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestRuleStore(Dictionary<string, LeaveRequestRule> items, List<string> order) : ILeaveRequestRuleStore
{
    public Dictionary<string, LeaveRequestRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestRule item)
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

    public List<LeaveRequestRule> List()
    {
        var outValue = new List<LeaveRequestRule>();
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
public sealed class CachedLeaveRequestRuleStore(MemoryLeaveRequestRuleStore inner, LeaveRequestRuleCache cache, int hits, int misses) : ILeaveRequestRuleStore
{
    public MemoryLeaveRequestRuleStore Inner { get; set; } = inner;
    public LeaveRequestRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestRule? Find(string code)
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

    public void Save(LeaveRequestRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request rules. Empty fields match all.</summary>
public sealed record LeaveRequestRuleQuery(string Country, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestRule item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<LeaveRequestRule> Apply(List<LeaveRequestRule> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request rule store.</summary>
public static class LeaveRequestRuleStore
{
    public static ILeaveRequestRuleStore NewMemoryLeaveRequestRuleStore()
    {
        return new MemoryLeaveRequestRuleStore(new Dictionary<string, LeaveRequestRule>(), new List<string>());
    }

    public static ILeaveRequestRuleStore NewCachedLeaveRequestRuleStore(MemoryLeaveRequestRuleStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestRuleStore(inner, LeaveRequestRuleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestRule> FindLeaveRequestRules(ILeaveRequestRuleStore store, LeaveRequestRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
