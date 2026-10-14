namespace Acme.Workforce.Compliance.Audit.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal audit assignments by code.</summary>
public interface ISeasonalAuditAssignmentStore
{
    SeasonalAuditAssignment? Find(string code);
    void Save(SeasonalAuditAssignment item);
    bool Remove(string code);
    List<SeasonalAuditAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalAuditAssignmentStore(Dictionary<string, SeasonalAuditAssignment> items, List<string> order) : ISeasonalAuditAssignmentStore
{
    public Dictionary<string, SeasonalAuditAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalAuditAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalAuditAssignment item)
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

    public List<SeasonalAuditAssignment> List()
    {
        var outValue = new List<SeasonalAuditAssignment>();
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
public sealed class CachedSeasonalAuditAssignmentStore(MemorySeasonalAuditAssignmentStore inner, SeasonalAuditAssignmentCache cache, int hits, int misses) : ISeasonalAuditAssignmentStore
{
    public MemorySeasonalAuditAssignmentStore Inner { get; set; } = inner;
    public SeasonalAuditAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalAuditAssignment? Find(string code)
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

    public void Save(SeasonalAuditAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalAuditAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal audit assignments. Empty fields match all.</summary>
public sealed record SeasonalAuditAssignmentQuery(string Unit, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalAuditAssignment item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<SeasonalAuditAssignment> Apply(List<SeasonalAuditAssignment> items)
    {
        var matched = Collections.Filter(items, (SeasonalAuditAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal audit assignment store.</summary>
public static class SeasonalAuditAssignmentStore
{
    public static ISeasonalAuditAssignmentStore NewMemorySeasonalAuditAssignmentStore()
    {
        return new MemorySeasonalAuditAssignmentStore(
            new Dictionary<string, SeasonalAuditAssignment>(),
            new List<string>());
    }

    public static ISeasonalAuditAssignmentStore NewCachedSeasonalAuditAssignmentStore(MemorySeasonalAuditAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalAuditAssignmentStore(
            inner,
            SeasonalAuditAssignmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalAuditAssignment> FindSeasonalAuditAssignments(ISeasonalAuditAssignmentStore store, SeasonalAuditAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
