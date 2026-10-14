namespace Acme.Workforce.Absence.Balance.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled balance drafts by code.</summary>
public interface IScheduledBalanceDraftStore
{
    ScheduledBalanceDraft? Find(string code);
    void Save(ScheduledBalanceDraft item);
    bool Remove(string code);
    List<ScheduledBalanceDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledBalanceDraftStore(Dictionary<string, ScheduledBalanceDraft> items, List<string> order) : IScheduledBalanceDraftStore
{
    public Dictionary<string, ScheduledBalanceDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledBalanceDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledBalanceDraft item)
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

    public List<ScheduledBalanceDraft> List()
    {
        var outValue = new List<ScheduledBalanceDraft>();
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
public sealed class CachedScheduledBalanceDraftStore(MemoryScheduledBalanceDraftStore inner, ScheduledBalanceDraftCache cache, int hits, int misses) : IScheduledBalanceDraftStore
{
    public MemoryScheduledBalanceDraftStore Inner { get; set; } = inner;
    public ScheduledBalanceDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledBalanceDraft? Find(string code)
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

    public void Save(ScheduledBalanceDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledBalanceDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled balance drafts. Empty fields match all.</summary>
public sealed record ScheduledBalanceDraftQuery(string Name, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledBalanceDraft item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledBalanceDraft> Apply(List<ScheduledBalanceDraft> items)
    {
        var matched = Collections.Filter(items, (ScheduledBalanceDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled balance draft store.</summary>
public static class ScheduledBalanceDraftStore
{
    public static IScheduledBalanceDraftStore NewMemoryScheduledBalanceDraftStore()
    {
        return new MemoryScheduledBalanceDraftStore(
            new Dictionary<string, ScheduledBalanceDraft>(),
            new List<string>());
    }

    public static IScheduledBalanceDraftStore NewCachedScheduledBalanceDraftStore(MemoryScheduledBalanceDraftStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledBalanceDraftStore(inner, ScheduledBalanceDraftCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ScheduledBalanceDraft> FindScheduledBalanceDrafts(IScheduledBalanceDraftStore store, ScheduledBalanceDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
