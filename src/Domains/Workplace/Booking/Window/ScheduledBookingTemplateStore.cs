namespace Acme.Workforce.Workplace.Booking.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled booking templates by code.</summary>
public interface IScheduledBookingTemplateStore
{
    ScheduledBookingTemplate? Find(string code);
    void Save(ScheduledBookingTemplate item);
    bool Remove(string code);
    List<ScheduledBookingTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledBookingTemplateStore(Dictionary<string, ScheduledBookingTemplate> items, List<string> order) : IScheduledBookingTemplateStore
{
    public Dictionary<string, ScheduledBookingTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledBookingTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledBookingTemplate item)
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

    public List<ScheduledBookingTemplate> List()
    {
        var outValue = new List<ScheduledBookingTemplate>();
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
public sealed class CachedScheduledBookingTemplateStore(MemoryScheduledBookingTemplateStore inner, ScheduledBookingTemplateCache cache, int hits, int misses) : IScheduledBookingTemplateStore
{
    public MemoryScheduledBookingTemplateStore Inner { get; set; } = inner;
    public ScheduledBookingTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledBookingTemplate? Find(string code)
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

    public void Save(ScheduledBookingTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledBookingTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled booking templates. Empty fields match all.</summary>
public sealed record ScheduledBookingTemplateQuery(string Name, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledBookingTemplate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<ScheduledBookingTemplate> Apply(List<ScheduledBookingTemplate> items)
    {
        var matched = Collections.Filter(items, (ScheduledBookingTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled booking template store.</summary>
public static class ScheduledBookingTemplateStore
{
    public static IScheduledBookingTemplateStore NewMemoryScheduledBookingTemplateStore()
    {
        return new MemoryScheduledBookingTemplateStore(
            new Dictionary<string, ScheduledBookingTemplate>(),
            new List<string>());
    }

    public static IScheduledBookingTemplateStore NewCachedScheduledBookingTemplateStore(MemoryScheduledBookingTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledBookingTemplateStore(
            inner,
            ScheduledBookingTemplateCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledBookingTemplate> FindScheduledBookingTemplates(IScheduledBookingTemplateStore store, ScheduledBookingTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
