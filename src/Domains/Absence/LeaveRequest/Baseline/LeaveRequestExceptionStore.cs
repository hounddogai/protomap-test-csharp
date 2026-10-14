namespace Acme.Workforce.Absence.LeaveRequest.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request exceptions by code.</summary>
public interface ILeaveRequestExceptionStore
{
    LeaveRequestException? Find(string code);
    void Save(LeaveRequestException item);
    bool Remove(string code);
    List<LeaveRequestException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestExceptionStore(Dictionary<string, LeaveRequestException> items, List<string> order) : ILeaveRequestExceptionStore
{
    public Dictionary<string, LeaveRequestException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestException item)
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

    public List<LeaveRequestException> List()
    {
        var outValue = new List<LeaveRequestException>();
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
public sealed class CachedLeaveRequestExceptionStore(MemoryLeaveRequestExceptionStore inner, LeaveRequestExceptionCache cache, int hits, int misses) : ILeaveRequestExceptionStore
{
    public MemoryLeaveRequestExceptionStore Inner { get; set; } = inner;
    public LeaveRequestExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestException? Find(string code)
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

    public void Save(LeaveRequestException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request exceptions. Empty fields match all.</summary>
public sealed record LeaveRequestExceptionQuery(string Label, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestException item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<LeaveRequestException> Apply(List<LeaveRequestException> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request exception store.</summary>
public static class LeaveRequestExceptionStore
{
    public static ILeaveRequestExceptionStore NewMemoryLeaveRequestExceptionStore()
    {
        return new MemoryLeaveRequestExceptionStore(
            new Dictionary<string, LeaveRequestException>(),
            new List<string>());
    }

    public static ILeaveRequestExceptionStore NewCachedLeaveRequestExceptionStore(MemoryLeaveRequestExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestExceptionStore(inner, LeaveRequestExceptionCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestException> FindLeaveRequestExceptions(ILeaveRequestExceptionStore store, LeaveRequestExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
