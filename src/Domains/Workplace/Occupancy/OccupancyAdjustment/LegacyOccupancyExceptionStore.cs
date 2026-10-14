namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy occupancy exceptions by code.</summary>
public interface ILegacyOccupancyExceptionStore
{
    LegacyOccupancyException? Find(string code);
    void Save(LegacyOccupancyException item);
    bool Remove(string code);
    List<LegacyOccupancyException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyOccupancyExceptionStore(Dictionary<string, LegacyOccupancyException> items, List<string> order) : ILegacyOccupancyExceptionStore
{
    public Dictionary<string, LegacyOccupancyException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyOccupancyException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyOccupancyException item)
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

    public List<LegacyOccupancyException> List()
    {
        var outValue = new List<LegacyOccupancyException>();
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
public sealed class CachedLegacyOccupancyExceptionStore(MemoryLegacyOccupancyExceptionStore inner, LegacyOccupancyExceptionCache cache, int hits, int misses) : ILegacyOccupancyExceptionStore
{
    public MemoryLegacyOccupancyExceptionStore Inner { get; set; } = inner;
    public LegacyOccupancyExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyOccupancyException? Find(string code)
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

    public void Save(LegacyOccupancyException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyOccupancyException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy occupancy exceptions. Empty fields match all.</summary>
public sealed record LegacyOccupancyExceptionQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyOccupancyException item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<LegacyOccupancyException> Apply(List<LegacyOccupancyException> items)
    {
        var matched = Collections.Filter(items, (LegacyOccupancyException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy occupancy exception store.</summary>
public static class LegacyOccupancyExceptionStore
{
    public static ILegacyOccupancyExceptionStore NewMemoryLegacyOccupancyExceptionStore()
    {
        return new MemoryLegacyOccupancyExceptionStore(
            new Dictionary<string, LegacyOccupancyException>(),
            new List<string>());
    }

    public static ILegacyOccupancyExceptionStore NewCachedLegacyOccupancyExceptionStore(MemoryLegacyOccupancyExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyOccupancyExceptionStore(
            inner,
            LegacyOccupancyExceptionCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LegacyOccupancyException> FindLegacyOccupancyExceptions(ILegacyOccupancyExceptionStore store, LegacyOccupancyExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
