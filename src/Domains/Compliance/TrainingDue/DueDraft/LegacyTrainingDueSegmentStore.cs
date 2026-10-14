namespace Acme.Workforce.Compliance.TrainingDue.DueDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy training due segments by code.</summary>
public interface ILegacyTrainingDueSegmentStore
{
    LegacyTrainingDueSegment? Find(string code);
    void Save(LegacyTrainingDueSegment item);
    bool Remove(string code);
    List<LegacyTrainingDueSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyTrainingDueSegmentStore(Dictionary<string, LegacyTrainingDueSegment> items, List<string> order) : ILegacyTrainingDueSegmentStore
{
    public Dictionary<string, LegacyTrainingDueSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyTrainingDueSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyTrainingDueSegment item)
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

    public List<LegacyTrainingDueSegment> List()
    {
        var outValue = new List<LegacyTrainingDueSegment>();
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
public sealed class CachedLegacyTrainingDueSegmentStore(MemoryLegacyTrainingDueSegmentStore inner, LegacyTrainingDueSegmentCache cache, int hits, int misses) : ILegacyTrainingDueSegmentStore
{
    public MemoryLegacyTrainingDueSegmentStore Inner { get; set; } = inner;
    public LegacyTrainingDueSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyTrainingDueSegment? Find(string code)
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

    public void Save(LegacyTrainingDueSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyTrainingDueSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy training due segments. Empty fields match all.</summary>
public sealed record LegacyTrainingDueSegmentQuery(string Label, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyTrainingDueSegment item)
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

    public List<LegacyTrainingDueSegment> Apply(List<LegacyTrainingDueSegment> items)
    {
        var matched = Collections.Filter(items, (LegacyTrainingDueSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy training due segment store.</summary>
public static class LegacyTrainingDueSegmentStore
{
    public static ILegacyTrainingDueSegmentStore NewMemoryLegacyTrainingDueSegmentStore()
    {
        return new MemoryLegacyTrainingDueSegmentStore(
            new Dictionary<string, LegacyTrainingDueSegment>(),
            new List<string>());
    }

    public static ILegacyTrainingDueSegmentStore NewCachedLegacyTrainingDueSegmentStore(MemoryLegacyTrainingDueSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyTrainingDueSegmentStore(
            inner,
            LegacyTrainingDueSegmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LegacyTrainingDueSegment> FindLegacyTrainingDueSegments(ILegacyTrainingDueSegmentStore store, LegacyTrainingDueSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
