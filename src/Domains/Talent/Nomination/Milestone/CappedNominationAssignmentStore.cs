namespace Acme.Workforce.Talent.Nomination.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped nomination assignments by code.</summary>
public interface ICappedNominationAssignmentStore
{
    CappedNominationAssignment? Find(string code);
    void Save(CappedNominationAssignment item);
    bool Remove(string code);
    List<CappedNominationAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedNominationAssignmentStore(Dictionary<string, CappedNominationAssignment> items, List<string> order) : ICappedNominationAssignmentStore
{
    public Dictionary<string, CappedNominationAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedNominationAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedNominationAssignment item)
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

    public List<CappedNominationAssignment> List()
    {
        var outValue = new List<CappedNominationAssignment>();
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
public sealed class CachedCappedNominationAssignmentStore(MemoryCappedNominationAssignmentStore inner, CappedNominationAssignmentCache cache, int hits, int misses) : ICappedNominationAssignmentStore
{
    public MemoryCappedNominationAssignmentStore Inner { get; set; } = inner;
    public CappedNominationAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedNominationAssignment? Find(string code)
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

    public void Save(CappedNominationAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedNominationAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped nomination assignments. Empty fields match all.</summary>
public sealed record CappedNominationAssignmentQuery(string Country, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(CappedNominationAssignment item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedNominationAssignment> Apply(List<CappedNominationAssignment> items)
    {
        var matched = Collections.Filter(items, (CappedNominationAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped nomination assignment store.</summary>
public static class CappedNominationAssignmentStore
{
    public static ICappedNominationAssignmentStore NewMemoryCappedNominationAssignmentStore()
    {
        return new MemoryCappedNominationAssignmentStore(
            new Dictionary<string, CappedNominationAssignment>(),
            new List<string>());
    }

    public static ICappedNominationAssignmentStore NewCachedCappedNominationAssignmentStore(MemoryCappedNominationAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedCappedNominationAssignmentStore(
            inner,
            CappedNominationAssignmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<CappedNominationAssignment> FindCappedNominationAssignments(ICappedNominationAssignmentStore store, CappedNominationAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
