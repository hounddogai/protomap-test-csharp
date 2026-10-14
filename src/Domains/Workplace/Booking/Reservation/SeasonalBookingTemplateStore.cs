namespace Acme.Workforce.Workplace.Booking.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal booking templates by code.</summary>
public interface ISeasonalBookingTemplateStore
{
    SeasonalBookingTemplate? Find(string code);
    void Save(SeasonalBookingTemplate item);
    bool Remove(string code);
    List<SeasonalBookingTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalBookingTemplateStore(Dictionary<string, SeasonalBookingTemplate> items, List<string> order) : ISeasonalBookingTemplateStore
{
    public Dictionary<string, SeasonalBookingTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalBookingTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalBookingTemplate item)
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

    public List<SeasonalBookingTemplate> List()
    {
        var outValue = new List<SeasonalBookingTemplate>();
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
public sealed class CachedSeasonalBookingTemplateStore(MemorySeasonalBookingTemplateStore inner, SeasonalBookingTemplateCache cache, int hits, int misses) : ISeasonalBookingTemplateStore
{
    public MemorySeasonalBookingTemplateStore Inner { get; set; } = inner;
    public SeasonalBookingTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalBookingTemplate? Find(string code)
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

    public void Save(SeasonalBookingTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalBookingTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal booking templates. Empty fields match all.</summary>
public sealed record SeasonalBookingTemplateQuery(string Segment, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalBookingTemplate item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<SeasonalBookingTemplate> Apply(List<SeasonalBookingTemplate> items)
    {
        var matched = Collections.Filter(items, (SeasonalBookingTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal booking template store.</summary>
public static class SeasonalBookingTemplateStore
{
    public static ISeasonalBookingTemplateStore NewMemorySeasonalBookingTemplateStore()
    {
        return new MemorySeasonalBookingTemplateStore(
            new Dictionary<string, SeasonalBookingTemplate>(),
            new List<string>());
    }

    public static ISeasonalBookingTemplateStore NewCachedSeasonalBookingTemplateStore(MemorySeasonalBookingTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalBookingTemplateStore(
            inner,
            SeasonalBookingTemplateCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalBookingTemplate> FindSeasonalBookingTemplates(ISeasonalBookingTemplateStore store, SeasonalBookingTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
