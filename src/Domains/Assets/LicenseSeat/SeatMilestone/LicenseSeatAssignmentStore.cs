namespace Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps license seat assignments by code.</summary>
public interface ILicenseSeatAssignmentStore
{
    LicenseSeatAssignment? Find(string code);
    void Save(LicenseSeatAssignment item);
    bool Remove(string code);
    List<LicenseSeatAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLicenseSeatAssignmentStore(Dictionary<string, LicenseSeatAssignment> items, List<string> order) : ILicenseSeatAssignmentStore
{
    public Dictionary<string, LicenseSeatAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LicenseSeatAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LicenseSeatAssignment item)
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

    public List<LicenseSeatAssignment> List()
    {
        var outValue = new List<LicenseSeatAssignment>();
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
public sealed class CachedLicenseSeatAssignmentStore(MemoryLicenseSeatAssignmentStore inner, LicenseSeatAssignmentCache cache, int hits, int misses) : ILicenseSeatAssignmentStore
{
    public MemoryLicenseSeatAssignmentStore Inner { get; set; } = inner;
    public LicenseSeatAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LicenseSeatAssignment? Find(string code)
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

    public void Save(LicenseSeatAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LicenseSeatAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects license seat assignments. Empty fields match all.</summary>
public sealed record LicenseSeatAssignmentQuery(string Locale, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(LicenseSeatAssignment item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LicenseSeatAssignment> Apply(List<LicenseSeatAssignment> items)
    {
        var matched = Collections.Filter(items, (LicenseSeatAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of license seat assignment store.</summary>
public static class LicenseSeatAssignmentStore
{
    public static ILicenseSeatAssignmentStore NewMemoryLicenseSeatAssignmentStore()
    {
        return new MemoryLicenseSeatAssignmentStore(
            new Dictionary<string, LicenseSeatAssignment>(),
            new List<string>());
    }

    public static ILicenseSeatAssignmentStore NewCachedLicenseSeatAssignmentStore(MemoryLicenseSeatAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedLicenseSeatAssignmentStore(inner, LicenseSeatAssignmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LicenseSeatAssignment> FindLicenseSeatAssignments(ILicenseSeatAssignmentStore store, LicenseSeatAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
