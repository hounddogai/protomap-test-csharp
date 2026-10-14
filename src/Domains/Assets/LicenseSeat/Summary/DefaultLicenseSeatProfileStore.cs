namespace Acme.Workforce.Assets.LicenseSeat.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default license seat profiles by code.</summary>
public interface IDefaultLicenseSeatProfileStore
{
    DefaultLicenseSeatProfile? Find(string code);
    void Save(DefaultLicenseSeatProfile item);
    bool Remove(string code);
    List<DefaultLicenseSeatProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultLicenseSeatProfileStore(Dictionary<string, DefaultLicenseSeatProfile> items, List<string> order) : IDefaultLicenseSeatProfileStore
{
    public Dictionary<string, DefaultLicenseSeatProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultLicenseSeatProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultLicenseSeatProfile item)
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

    public List<DefaultLicenseSeatProfile> List()
    {
        var outValue = new List<DefaultLicenseSeatProfile>();
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
public sealed class CachedDefaultLicenseSeatProfileStore(MemoryDefaultLicenseSeatProfileStore inner, DefaultLicenseSeatProfileCache cache, int hits, int misses) : IDefaultLicenseSeatProfileStore
{
    public MemoryDefaultLicenseSeatProfileStore Inner { get; set; } = inner;
    public DefaultLicenseSeatProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultLicenseSeatProfile? Find(string code)
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

    public void Save(DefaultLicenseSeatProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultLicenseSeatProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default license seat profiles. Empty fields match all.</summary>
public sealed record DefaultLicenseSeatProfileQuery(string Source, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultLicenseSeatProfile item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultLicenseSeatProfile> Apply(List<DefaultLicenseSeatProfile> items)
    {
        var matched = Collections.Filter(items, (DefaultLicenseSeatProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default license seat profile store.</summary>
public static class DefaultLicenseSeatProfileStore
{
    public static IDefaultLicenseSeatProfileStore NewMemoryDefaultLicenseSeatProfileStore()
    {
        return new MemoryDefaultLicenseSeatProfileStore(
            new Dictionary<string, DefaultLicenseSeatProfile>(),
            new List<string>());
    }

    public static IDefaultLicenseSeatProfileStore NewCachedDefaultLicenseSeatProfileStore(MemoryDefaultLicenseSeatProfileStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultLicenseSeatProfileStore(
            inner,
            DefaultLicenseSeatProfileCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<DefaultLicenseSeatProfile> FindDefaultLicenseSeatProfiles(IDefaultLicenseSeatProfileStore store, DefaultLicenseSeatProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
