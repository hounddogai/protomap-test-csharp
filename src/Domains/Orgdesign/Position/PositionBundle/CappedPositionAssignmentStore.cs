namespace Acme.Workforce.Orgdesign.Position.PositionBundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped position assignments by code.</summary>
public interface ICappedPositionAssignmentStore
{
    CappedPositionAssignment? Find(string code);
    void Save(CappedPositionAssignment item);
    bool Remove(string code);
    List<CappedPositionAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedPositionAssignmentStore(Dictionary<string, CappedPositionAssignment> items, List<string> order) : ICappedPositionAssignmentStore
{
    public Dictionary<string, CappedPositionAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedPositionAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedPositionAssignment item)
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

    public List<CappedPositionAssignment> List()
    {
        var outValue = new List<CappedPositionAssignment>();
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
public sealed class CachedCappedPositionAssignmentStore(MemoryCappedPositionAssignmentStore inner, CappedPositionAssignmentCache cache, int hits, int misses) : ICappedPositionAssignmentStore
{
    public MemoryCappedPositionAssignmentStore Inner { get; set; } = inner;
    public CappedPositionAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedPositionAssignment? Find(string code)
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

    public void Save(CappedPositionAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedPositionAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped position assignments. Empty fields match all.</summary>
public sealed record CappedPositionAssignmentQuery(string Locale, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(CappedPositionAssignment item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedPositionAssignment> Apply(List<CappedPositionAssignment> items)
    {
        var matched = Collections.Filter(items, (CappedPositionAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped position assignment store.</summary>
public static class CappedPositionAssignmentStore
{
    public static ICappedPositionAssignmentStore NewMemoryCappedPositionAssignmentStore()
    {
        return new MemoryCappedPositionAssignmentStore(
            new Dictionary<string, CappedPositionAssignment>(),
            new List<string>());
    }

    public static ICappedPositionAssignmentStore NewCachedCappedPositionAssignmentStore(MemoryCappedPositionAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedCappedPositionAssignmentStore(
            inner,
            CappedPositionAssignmentCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<CappedPositionAssignment> FindCappedPositionAssignments(ICappedPositionAssignmentStore store, CappedPositionAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
