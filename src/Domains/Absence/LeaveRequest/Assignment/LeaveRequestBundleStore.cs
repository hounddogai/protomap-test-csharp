namespace Acme.Workforce.Absence.LeaveRequest.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request bundles by code.</summary>
public interface ILeaveRequestBundleStore
{
    LeaveRequestBundle? Find(string code);
    void Save(LeaveRequestBundle item);
    bool Remove(string code);
    List<LeaveRequestBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestBundleStore(Dictionary<string, LeaveRequestBundle> items, List<string> order) : ILeaveRequestBundleStore
{
    public Dictionary<string, LeaveRequestBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestBundle item)
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

    public List<LeaveRequestBundle> List()
    {
        var outValue = new List<LeaveRequestBundle>();
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
public sealed class CachedLeaveRequestBundleStore(MemoryLeaveRequestBundleStore inner, LeaveRequestBundleCache cache, int hits, int misses) : ILeaveRequestBundleStore
{
    public MemoryLeaveRequestBundleStore Inner { get; set; } = inner;
    public LeaveRequestBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestBundle? Find(string code)
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

    public void Save(LeaveRequestBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request bundles. Empty fields match all.</summary>
public sealed record LeaveRequestBundleQuery(string Source, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestBundle item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LeaveRequestBundle> Apply(List<LeaveRequestBundle> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request bundle store.</summary>
public static class LeaveRequestBundleStore
{
    public static ILeaveRequestBundleStore NewMemoryLeaveRequestBundleStore()
    {
        return new MemoryLeaveRequestBundleStore(new Dictionary<string, LeaveRequestBundle>(), new List<string>());
    }

    public static ILeaveRequestBundleStore NewCachedLeaveRequestBundleStore(MemoryLeaveRequestBundleStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestBundleStore(inner, LeaveRequestBundleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestBundle> FindLeaveRequestBundles(ILeaveRequestBundleStore store, LeaveRequestBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
