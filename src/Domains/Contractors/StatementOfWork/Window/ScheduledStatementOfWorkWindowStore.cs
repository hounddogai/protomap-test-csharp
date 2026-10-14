namespace Acme.Workforce.Contractors.StatementOfWork.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled statement of work windows by code.</summary>
public interface IScheduledStatementOfWorkWindowStore
{
    ScheduledStatementOfWorkWindow? Find(string code);
    void Save(ScheduledStatementOfWorkWindow item);
    bool Remove(string code);
    List<ScheduledStatementOfWorkWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledStatementOfWorkWindowStore(Dictionary<string, ScheduledStatementOfWorkWindow> items, List<string> order) : IScheduledStatementOfWorkWindowStore
{
    public Dictionary<string, ScheduledStatementOfWorkWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledStatementOfWorkWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledStatementOfWorkWindow item)
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

    public List<ScheduledStatementOfWorkWindow> List()
    {
        var outValue = new List<ScheduledStatementOfWorkWindow>();
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
public sealed class CachedScheduledStatementOfWorkWindowStore(MemoryScheduledStatementOfWorkWindowStore inner, ScheduledStatementOfWorkWindowCache cache, int hits, int misses) : IScheduledStatementOfWorkWindowStore
{
    public MemoryScheduledStatementOfWorkWindowStore Inner { get; set; } = inner;
    public ScheduledStatementOfWorkWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledStatementOfWorkWindow? Find(string code)
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

    public void Save(ScheduledStatementOfWorkWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledStatementOfWorkWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled statement of work windows. Empty fields match all.</summary>
public sealed record ScheduledStatementOfWorkWindowQuery(string Country, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledStatementOfWorkWindow item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<ScheduledStatementOfWorkWindow> Apply(List<ScheduledStatementOfWorkWindow> items)
    {
        var matched = Collections.Filter(items, (ScheduledStatementOfWorkWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled statement of work window store.</summary>
public static class ScheduledStatementOfWorkWindowStore
{
    public static IScheduledStatementOfWorkWindowStore NewMemoryScheduledStatementOfWorkWindowStore()
    {
        return new MemoryScheduledStatementOfWorkWindowStore(
            new Dictionary<string, ScheduledStatementOfWorkWindow>(),
            new List<string>());
    }

    public static IScheduledStatementOfWorkWindowStore NewCachedScheduledStatementOfWorkWindowStore(MemoryScheduledStatementOfWorkWindowStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledStatementOfWorkWindowStore(
            inner,
            ScheduledStatementOfWorkWindowCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledStatementOfWorkWindow> FindScheduledStatementOfWorkWindows(IScheduledStatementOfWorkWindowStore store, ScheduledStatementOfWorkWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
