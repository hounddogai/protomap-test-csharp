namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled retro pay entries by code.</summary>
public interface IScheduledRetroPayEntryStore
{
    ScheduledRetroPayEntry? Find(string code);
    void Save(ScheduledRetroPayEntry item);
    bool Remove(string code);
    List<ScheduledRetroPayEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledRetroPayEntryStore(Dictionary<string, ScheduledRetroPayEntry> items, List<string> order) : IScheduledRetroPayEntryStore
{
    public Dictionary<string, ScheduledRetroPayEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledRetroPayEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledRetroPayEntry item)
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

    public List<ScheduledRetroPayEntry> List()
    {
        var outValue = new List<ScheduledRetroPayEntry>();
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
public sealed class CachedScheduledRetroPayEntryStore(MemoryScheduledRetroPayEntryStore inner, ScheduledRetroPayEntryCache cache, int hits, int misses) : IScheduledRetroPayEntryStore
{
    public MemoryScheduledRetroPayEntryStore Inner { get; set; } = inner;
    public ScheduledRetroPayEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledRetroPayEntry? Find(string code)
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

    public void Save(ScheduledRetroPayEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledRetroPayEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled retro pay entries. Empty fields match all.</summary>
public sealed record ScheduledRetroPayEntryQuery(string Title, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledRetroPayEntry item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledRetroPayEntry> Apply(List<ScheduledRetroPayEntry> items)
    {
        var matched = Collections.Filter(items, (ScheduledRetroPayEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled retro pay entry store.</summary>
public static class ScheduledRetroPayEntryStore
{
    public static IScheduledRetroPayEntryStore NewMemoryScheduledRetroPayEntryStore()
    {
        return new MemoryScheduledRetroPayEntryStore(
            new Dictionary<string, ScheduledRetroPayEntry>(),
            new List<string>());
    }

    public static IScheduledRetroPayEntryStore NewCachedScheduledRetroPayEntryStore(MemoryScheduledRetroPayEntryStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledRetroPayEntryStore(
            inner,
            ScheduledRetroPayEntryCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledRetroPayEntry> FindScheduledRetroPayEntries(IScheduledRetroPayEntryStore store, ScheduledRetroPayEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
