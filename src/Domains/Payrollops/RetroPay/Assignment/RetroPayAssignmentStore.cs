namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay assignments by code.</summary>
public interface IRetroPayAssignmentStore
{
    RetroPayAssignment? Find(string code);
    void Save(RetroPayAssignment item);
    bool Remove(string code);
    List<RetroPayAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayAssignmentStore(Dictionary<string, RetroPayAssignment> items, List<string> order) : IRetroPayAssignmentStore
{
    public Dictionary<string, RetroPayAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayAssignment item)
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

    public List<RetroPayAssignment> List()
    {
        var outValue = new List<RetroPayAssignment>();
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
public sealed class CachedRetroPayAssignmentStore(MemoryRetroPayAssignmentStore inner, RetroPayAssignmentCache cache, int hits, int misses) : IRetroPayAssignmentStore
{
    public MemoryRetroPayAssignmentStore Inner { get; set; } = inner;
    public RetroPayAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayAssignment? Find(string code)
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

    public void Save(RetroPayAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay assignments. Empty fields match all.</summary>
public sealed record RetroPayAssignmentQuery(string Label, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayAssignment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RetroPayAssignment> Apply(List<RetroPayAssignment> items)
    {
        var matched = Collections.Filter(items, (RetroPayAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay assignment store.</summary>
public static class RetroPayAssignmentStore
{
    public static IRetroPayAssignmentStore NewMemoryRetroPayAssignmentStore()
    {
        return new MemoryRetroPayAssignmentStore(new Dictionary<string, RetroPayAssignment>(), new List<string>());
    }

    public static IRetroPayAssignmentStore NewCachedRetroPayAssignmentStore(MemoryRetroPayAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayAssignmentStore(inner, RetroPayAssignmentCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RetroPayAssignment> FindRetroPayAssignments(IRetroPayAssignmentStore store, RetroPayAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
