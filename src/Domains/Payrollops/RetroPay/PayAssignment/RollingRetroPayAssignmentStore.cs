namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling retro pay assignments by code.</summary>
public interface IRollingRetroPayAssignmentStore
{
    RollingRetroPayAssignment? Find(string code);
    void Save(RollingRetroPayAssignment item);
    bool Remove(string code);
    List<RollingRetroPayAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingRetroPayAssignmentStore(Dictionary<string, RollingRetroPayAssignment> items, List<string> order) : IRollingRetroPayAssignmentStore
{
    public Dictionary<string, RollingRetroPayAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingRetroPayAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingRetroPayAssignment item)
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

    public List<RollingRetroPayAssignment> List()
    {
        var outValue = new List<RollingRetroPayAssignment>();
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
public sealed class CachedRollingRetroPayAssignmentStore(MemoryRollingRetroPayAssignmentStore inner, RollingRetroPayAssignmentCache cache, int hits, int misses) : IRollingRetroPayAssignmentStore
{
    public MemoryRollingRetroPayAssignmentStore Inner { get; set; } = inner;
    public RollingRetroPayAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingRetroPayAssignment? Find(string code)
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

    public void Save(RollingRetroPayAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingRetroPayAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling retro pay assignments. Empty fields match all.</summary>
public sealed record RollingRetroPayAssignmentQuery(string Locale, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(RollingRetroPayAssignment item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingRetroPayAssignment> Apply(List<RollingRetroPayAssignment> items)
    {
        var matched = Collections.Filter(items, (RollingRetroPayAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling retro pay assignment store.</summary>
public static class RollingRetroPayAssignmentStore
{
    public static IRollingRetroPayAssignmentStore NewMemoryRollingRetroPayAssignmentStore()
    {
        return new MemoryRollingRetroPayAssignmentStore(
            new Dictionary<string, RollingRetroPayAssignment>(),
            new List<string>());
    }

    public static IRollingRetroPayAssignmentStore NewCachedRollingRetroPayAssignmentStore(MemoryRollingRetroPayAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedRollingRetroPayAssignmentStore(
            inner,
            RollingRetroPayAssignmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RollingRetroPayAssignment> FindRollingRetroPayAssignments(IRollingRetroPayAssignmentStore store, RollingRetroPayAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
