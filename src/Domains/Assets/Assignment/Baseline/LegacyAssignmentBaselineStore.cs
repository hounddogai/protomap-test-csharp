namespace Acme.Workforce.Assets.Assignment.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy assignment baselines by code.</summary>
public interface ILegacyAssignmentBaselineStore
{
    LegacyAssignmentBaseline? Find(string code);
    void Save(LegacyAssignmentBaseline item);
    bool Remove(string code);
    List<LegacyAssignmentBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyAssignmentBaselineStore(Dictionary<string, LegacyAssignmentBaseline> items, List<string> order) : ILegacyAssignmentBaselineStore
{
    public Dictionary<string, LegacyAssignmentBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyAssignmentBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyAssignmentBaseline item)
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

    public List<LegacyAssignmentBaseline> List()
    {
        var outValue = new List<LegacyAssignmentBaseline>();
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
public sealed class CachedLegacyAssignmentBaselineStore(MemoryLegacyAssignmentBaselineStore inner, LegacyAssignmentBaselineCache cache, int hits, int misses) : ILegacyAssignmentBaselineStore
{
    public MemoryLegacyAssignmentBaselineStore Inner { get; set; } = inner;
    public LegacyAssignmentBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyAssignmentBaseline? Find(string code)
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

    public void Save(LegacyAssignmentBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyAssignmentBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy assignment baselines. Empty fields match all.</summary>
public sealed record LegacyAssignmentBaselineQuery(string Segment, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyAssignmentBaseline item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyAssignmentBaseline> Apply(List<LegacyAssignmentBaseline> items)
    {
        var matched = Collections.Filter(items, (LegacyAssignmentBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy assignment baseline store.</summary>
public static class LegacyAssignmentBaselineStore
{
    public static ILegacyAssignmentBaselineStore NewMemoryLegacyAssignmentBaselineStore()
    {
        return new MemoryLegacyAssignmentBaselineStore(
            new Dictionary<string, LegacyAssignmentBaseline>(),
            new List<string>());
    }

    public static ILegacyAssignmentBaselineStore NewCachedLegacyAssignmentBaselineStore(MemoryLegacyAssignmentBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyAssignmentBaselineStore(
            inner,
            LegacyAssignmentBaselineCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LegacyAssignmentBaseline> FindLegacyAssignmentBaselines(ILegacyAssignmentBaselineStore store, LegacyAssignmentBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
