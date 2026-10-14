namespace Acme.Workforce.Assets.Refresh.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled refresh mappings by code.</summary>
public interface IScheduledRefreshMappingStore
{
    ScheduledRefreshMapping? Find(string code);
    void Save(ScheduledRefreshMapping item);
    bool Remove(string code);
    List<ScheduledRefreshMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledRefreshMappingStore(Dictionary<string, ScheduledRefreshMapping> items, List<string> order) : IScheduledRefreshMappingStore
{
    public Dictionary<string, ScheduledRefreshMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledRefreshMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledRefreshMapping item)
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

    public List<ScheduledRefreshMapping> List()
    {
        var outValue = new List<ScheduledRefreshMapping>();
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
public sealed class CachedScheduledRefreshMappingStore(MemoryScheduledRefreshMappingStore inner, ScheduledRefreshMappingCache cache, int hits, int misses) : IScheduledRefreshMappingStore
{
    public MemoryScheduledRefreshMappingStore Inner { get; set; } = inner;
    public ScheduledRefreshMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledRefreshMapping? Find(string code)
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

    public void Save(ScheduledRefreshMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledRefreshMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled refresh mappings. Empty fields match all.</summary>
public sealed record ScheduledRefreshMappingQuery(string Team, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledRefreshMapping item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledRefreshMapping> Apply(List<ScheduledRefreshMapping> items)
    {
        var matched = Collections.Filter(items, (ScheduledRefreshMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled refresh mapping store.</summary>
public static class ScheduledRefreshMappingStore
{
    public static IScheduledRefreshMappingStore NewMemoryScheduledRefreshMappingStore()
    {
        return new MemoryScheduledRefreshMappingStore(
            new Dictionary<string, ScheduledRefreshMapping>(),
            new List<string>());
    }

    public static IScheduledRefreshMappingStore NewCachedScheduledRefreshMappingStore(MemoryScheduledRefreshMappingStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledRefreshMappingStore(
            inner,
            ScheduledRefreshMappingCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledRefreshMapping> FindScheduledRefreshMappings(IScheduledRefreshMappingStore store, ScheduledRefreshMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
