namespace Acme.Workforce.Compensation.Adjustment.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal adjustment assignments by code.</summary>
public interface ISeasonalAdjustmentAssignmentStore
{
    SeasonalAdjustmentAssignment? Find(string code);
    void Save(SeasonalAdjustmentAssignment item);
    bool Remove(string code);
    List<SeasonalAdjustmentAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalAdjustmentAssignmentStore(Dictionary<string, SeasonalAdjustmentAssignment> items, List<string> order) : ISeasonalAdjustmentAssignmentStore
{
    public Dictionary<string, SeasonalAdjustmentAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalAdjustmentAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalAdjustmentAssignment item)
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

    public List<SeasonalAdjustmentAssignment> List()
    {
        var outValue = new List<SeasonalAdjustmentAssignment>();
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
public sealed class CachedSeasonalAdjustmentAssignmentStore(MemorySeasonalAdjustmentAssignmentStore inner, SeasonalAdjustmentAssignmentCache cache, int hits, int misses) : ISeasonalAdjustmentAssignmentStore
{
    public MemorySeasonalAdjustmentAssignmentStore Inner { get; set; } = inner;
    public SeasonalAdjustmentAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalAdjustmentAssignment? Find(string code)
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

    public void Save(SeasonalAdjustmentAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalAdjustmentAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal adjustment assignments. Empty fields match all.</summary>
public sealed record SeasonalAdjustmentAssignmentQuery(string Title, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalAdjustmentAssignment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalAdjustmentAssignment> Apply(List<SeasonalAdjustmentAssignment> items)
    {
        var matched = Collections.Filter(items, (SeasonalAdjustmentAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal adjustment assignment store.</summary>
public static class SeasonalAdjustmentAssignmentStore
{
    public static ISeasonalAdjustmentAssignmentStore NewMemorySeasonalAdjustmentAssignmentStore()
    {
        return new MemorySeasonalAdjustmentAssignmentStore(
            new Dictionary<string, SeasonalAdjustmentAssignment>(),
            new List<string>());
    }

    public static ISeasonalAdjustmentAssignmentStore NewCachedSeasonalAdjustmentAssignmentStore(MemorySeasonalAdjustmentAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalAdjustmentAssignmentStore(
            inner,
            SeasonalAdjustmentAssignmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalAdjustmentAssignment> FindSeasonalAdjustmentAssignments(ISeasonalAdjustmentAssignmentStore store, SeasonalAdjustmentAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
