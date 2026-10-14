namespace Acme.Workforce.Talent.Nomination.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination assignments by code.</summary>
public interface INominationAssignmentStore
{
    NominationAssignment? Find(string code);
    void Save(NominationAssignment item);
    bool Remove(string code);
    List<NominationAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationAssignmentStore(Dictionary<string, NominationAssignment> items, List<string> order) : INominationAssignmentStore
{
    public Dictionary<string, NominationAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationAssignment item)
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

    public List<NominationAssignment> List()
    {
        var outValue = new List<NominationAssignment>();
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
public sealed class CachedNominationAssignmentStore(MemoryNominationAssignmentStore inner, NominationAssignmentCache cache, int hits, int misses) : INominationAssignmentStore
{
    public MemoryNominationAssignmentStore Inner { get; set; } = inner;
    public NominationAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationAssignment? Find(string code)
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

    public void Save(NominationAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination assignments. Empty fields match all.</summary>
public sealed record NominationAssignmentQuery(string Channel, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(NominationAssignment item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<NominationAssignment> Apply(List<NominationAssignment> items)
    {
        var matched = Collections.Filter(items, (NominationAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination assignment store.</summary>
public static class NominationAssignmentStore
{
    public static INominationAssignmentStore NewMemoryNominationAssignmentStore()
    {
        return new MemoryNominationAssignmentStore(
            new Dictionary<string, NominationAssignment>(),
            new List<string>());
    }

    public static INominationAssignmentStore NewCachedNominationAssignmentStore(MemoryNominationAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedNominationAssignmentStore(inner, NominationAssignmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<NominationAssignment> FindNominationAssignments(INominationAssignmentStore store, NominationAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
