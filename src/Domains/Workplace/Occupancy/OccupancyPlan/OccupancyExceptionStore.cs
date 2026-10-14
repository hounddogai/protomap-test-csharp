namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy exceptions by code.</summary>
public interface IOccupancyExceptionStore
{
    OccupancyException? Find(string code);
    void Save(OccupancyException item);
    bool Remove(string code);
    List<OccupancyException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyExceptionStore(Dictionary<string, OccupancyException> items, List<string> order) : IOccupancyExceptionStore
{
    public Dictionary<string, OccupancyException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyException item)
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

    public List<OccupancyException> List()
    {
        var outValue = new List<OccupancyException>();
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
public sealed class CachedOccupancyExceptionStore(MemoryOccupancyExceptionStore inner, OccupancyExceptionCache cache, int hits, int misses) : IOccupancyExceptionStore
{
    public MemoryOccupancyExceptionStore Inner { get; set; } = inner;
    public OccupancyExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyException? Find(string code)
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

    public void Save(OccupancyException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy exceptions. Empty fields match all.</summary>
public sealed record OccupancyExceptionQuery(string Label, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyException item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OccupancyException> Apply(List<OccupancyException> items)
    {
        var matched = Collections.Filter(items, (OccupancyException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy exception store.</summary>
public static class OccupancyExceptionStore
{
    public static IOccupancyExceptionStore NewMemoryOccupancyExceptionStore()
    {
        return new MemoryOccupancyExceptionStore(new Dictionary<string, OccupancyException>(), new List<string>());
    }

    public static IOccupancyExceptionStore NewCachedOccupancyExceptionStore(MemoryOccupancyExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyExceptionStore(inner, OccupancyExceptionCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<OccupancyException> FindOccupancyExceptions(IOccupancyExceptionStore store, OccupancyExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
