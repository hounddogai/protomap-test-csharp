namespace Acme.Workforce.Absence.Holiday.HolidaySchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default holiday profiles by code.</summary>
public interface IDefaultHolidayProfileStore
{
    DefaultHolidayProfile? Find(string code);
    void Save(DefaultHolidayProfile item);
    bool Remove(string code);
    List<DefaultHolidayProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultHolidayProfileStore(Dictionary<string, DefaultHolidayProfile> items, List<string> order) : IDefaultHolidayProfileStore
{
    public Dictionary<string, DefaultHolidayProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultHolidayProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultHolidayProfile item)
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

    public List<DefaultHolidayProfile> List()
    {
        var outValue = new List<DefaultHolidayProfile>();
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
public sealed class CachedDefaultHolidayProfileStore(MemoryDefaultHolidayProfileStore inner, DefaultHolidayProfileCache cache, int hits, int misses) : IDefaultHolidayProfileStore
{
    public MemoryDefaultHolidayProfileStore Inner { get; set; } = inner;
    public DefaultHolidayProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultHolidayProfile? Find(string code)
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

    public void Save(DefaultHolidayProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultHolidayProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default holiday profiles. Empty fields match all.</summary>
public sealed record DefaultHolidayProfileQuery(string Channel, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultHolidayProfile item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<DefaultHolidayProfile> Apply(List<DefaultHolidayProfile> items)
    {
        var matched = Collections.Filter(items, (DefaultHolidayProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default holiday profile store.</summary>
public static class DefaultHolidayProfileStore
{
    public static IDefaultHolidayProfileStore NewMemoryDefaultHolidayProfileStore()
    {
        return new MemoryDefaultHolidayProfileStore(
            new Dictionary<string, DefaultHolidayProfile>(),
            new List<string>());
    }

    public static IDefaultHolidayProfileStore NewCachedDefaultHolidayProfileStore(MemoryDefaultHolidayProfileStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultHolidayProfileStore(inner, DefaultHolidayProfileCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<DefaultHolidayProfile> FindDefaultHolidayProfiles(IDefaultHolidayProfileStore store, DefaultHolidayProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
