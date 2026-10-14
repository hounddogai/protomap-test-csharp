namespace Acme.Workforce.Workplace.FloorPlan.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal floor plan records by code.</summary>
public interface ISeasonalFloorPlanRecordStore
{
    SeasonalFloorPlanRecord? Find(string code);
    void Save(SeasonalFloorPlanRecord item);
    bool Remove(string code);
    List<SeasonalFloorPlanRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalFloorPlanRecordStore(Dictionary<string, SeasonalFloorPlanRecord> items, List<string> order) : ISeasonalFloorPlanRecordStore
{
    public Dictionary<string, SeasonalFloorPlanRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalFloorPlanRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalFloorPlanRecord item)
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

    public List<SeasonalFloorPlanRecord> List()
    {
        var outValue = new List<SeasonalFloorPlanRecord>();
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
public sealed class CachedSeasonalFloorPlanRecordStore(MemorySeasonalFloorPlanRecordStore inner, SeasonalFloorPlanRecordCache cache, int hits, int misses) : ISeasonalFloorPlanRecordStore
{
    public MemorySeasonalFloorPlanRecordStore Inner { get; set; } = inner;
    public SeasonalFloorPlanRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalFloorPlanRecord? Find(string code)
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

    public void Save(SeasonalFloorPlanRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalFloorPlanRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal floor plan records. Empty fields match all.</summary>
public sealed record SeasonalFloorPlanRecordQuery(string Unit, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalFloorPlanRecord item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<SeasonalFloorPlanRecord> Apply(List<SeasonalFloorPlanRecord> items)
    {
        var matched = Collections.Filter(items, (SeasonalFloorPlanRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal floor plan record store.</summary>
public static class SeasonalFloorPlanRecordStore
{
    public static ISeasonalFloorPlanRecordStore NewMemorySeasonalFloorPlanRecordStore()
    {
        return new MemorySeasonalFloorPlanRecordStore(
            new Dictionary<string, SeasonalFloorPlanRecord>(),
            new List<string>());
    }

    public static ISeasonalFloorPlanRecordStore NewCachedSeasonalFloorPlanRecordStore(MemorySeasonalFloorPlanRecordStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalFloorPlanRecordStore(
            inner,
            SeasonalFloorPlanRecordCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalFloorPlanRecord> FindSeasonalFloorPlanRecords(ISeasonalFloorPlanRecordStore store, SeasonalFloorPlanRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
