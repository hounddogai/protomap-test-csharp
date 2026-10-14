namespace Acme.Workforce.Compensation.BonusPool.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool assignments by code.</summary>
public interface IBonusPoolAssignmentStore
{
    BonusPoolAssignment? Find(string code);
    void Save(BonusPoolAssignment item);
    bool Remove(string code);
    List<BonusPoolAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolAssignmentStore(Dictionary<string, BonusPoolAssignment> items, List<string> order) : IBonusPoolAssignmentStore
{
    public Dictionary<string, BonusPoolAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolAssignment item)
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

    public List<BonusPoolAssignment> List()
    {
        var outValue = new List<BonusPoolAssignment>();
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
public sealed class CachedBonusPoolAssignmentStore(MemoryBonusPoolAssignmentStore inner, BonusPoolAssignmentCache cache, int hits, int misses) : IBonusPoolAssignmentStore
{
    public MemoryBonusPoolAssignmentStore Inner { get; set; } = inner;
    public BonusPoolAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolAssignment? Find(string code)
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

    public void Save(BonusPoolAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool assignments. Empty fields match all.</summary>
public sealed record BonusPoolAssignmentQuery(string Title, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolAssignment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<BonusPoolAssignment> Apply(List<BonusPoolAssignment> items)
    {
        var matched = Collections.Filter(items, (BonusPoolAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool assignment store.</summary>
public static class BonusPoolAssignmentStore
{
    public static IBonusPoolAssignmentStore NewMemoryBonusPoolAssignmentStore()
    {
        return new MemoryBonusPoolAssignmentStore(new Dictionary<string, BonusPoolAssignment>(), new List<string>());
    }

    public static IBonusPoolAssignmentStore NewCachedBonusPoolAssignmentStore(MemoryBonusPoolAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolAssignmentStore(inner, BonusPoolAssignmentCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BonusPoolAssignment> FindBonusPoolAssignments(IBonusPoolAssignmentStore store, BonusPoolAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
