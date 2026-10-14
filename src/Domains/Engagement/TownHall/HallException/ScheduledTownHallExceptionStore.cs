namespace Acme.Workforce.Engagement.TownHall.HallException;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled town hall exceptions by code.</summary>
public interface IScheduledTownHallExceptionStore
{
    ScheduledTownHallException? Find(string code);
    void Save(ScheduledTownHallException item);
    bool Remove(string code);
    List<ScheduledTownHallException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledTownHallExceptionStore(Dictionary<string, ScheduledTownHallException> items, List<string> order) : IScheduledTownHallExceptionStore
{
    public Dictionary<string, ScheduledTownHallException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledTownHallException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledTownHallException item)
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

    public List<ScheduledTownHallException> List()
    {
        var outValue = new List<ScheduledTownHallException>();
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
public sealed class CachedScheduledTownHallExceptionStore(MemoryScheduledTownHallExceptionStore inner, ScheduledTownHallExceptionCache cache, int hits, int misses) : IScheduledTownHallExceptionStore
{
    public MemoryScheduledTownHallExceptionStore Inner { get; set; } = inner;
    public ScheduledTownHallExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledTownHallException? Find(string code)
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

    public void Save(ScheduledTownHallException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledTownHallException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled town hall exceptions. Empty fields match all.</summary>
public sealed record ScheduledTownHallExceptionQuery(string Name, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledTownHallException item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledTownHallException> Apply(List<ScheduledTownHallException> items)
    {
        var matched = Collections.Filter(items, (ScheduledTownHallException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled town hall exception store.</summary>
public static class ScheduledTownHallExceptionStore
{
    public static IScheduledTownHallExceptionStore NewMemoryScheduledTownHallExceptionStore()
    {
        return new MemoryScheduledTownHallExceptionStore(
            new Dictionary<string, ScheduledTownHallException>(),
            new List<string>());
    }

    public static IScheduledTownHallExceptionStore NewCachedScheduledTownHallExceptionStore(MemoryScheduledTownHallExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledTownHallExceptionStore(
            inner,
            ScheduledTownHallExceptionCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledTownHallException> FindScheduledTownHallExceptions(IScheduledTownHallExceptionStore store, ScheduledTownHallExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
