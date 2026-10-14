namespace Acme.Workforce.Assets.LicenseSeat.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps license seat profiles by code.</summary>
public interface ILicenseSeatProfileStore
{
    LicenseSeatProfile? Find(string code);
    void Save(LicenseSeatProfile item);
    bool Remove(string code);
    List<LicenseSeatProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLicenseSeatProfileStore(Dictionary<string, LicenseSeatProfile> items, List<string> order) : ILicenseSeatProfileStore
{
    public Dictionary<string, LicenseSeatProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LicenseSeatProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LicenseSeatProfile item)
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

    public List<LicenseSeatProfile> List()
    {
        var outValue = new List<LicenseSeatProfile>();
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
public sealed class CachedLicenseSeatProfileStore(MemoryLicenseSeatProfileStore inner, LicenseSeatProfileCache cache, int hits, int misses) : ILicenseSeatProfileStore
{
    public MemoryLicenseSeatProfileStore Inner { get; set; } = inner;
    public LicenseSeatProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LicenseSeatProfile? Find(string code)
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

    public void Save(LicenseSeatProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LicenseSeatProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects license seat profiles. Empty fields match all.</summary>
public sealed record LicenseSeatProfileQuery(string Category, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(LicenseSeatProfile item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<LicenseSeatProfile> Apply(List<LicenseSeatProfile> items)
    {
        var matched = Collections.Filter(items, (LicenseSeatProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of license seat profile store.</summary>
public static class LicenseSeatProfileStore
{
    public static ILicenseSeatProfileStore NewMemoryLicenseSeatProfileStore()
    {
        return new MemoryLicenseSeatProfileStore(new Dictionary<string, LicenseSeatProfile>(), new List<string>());
    }

    public static ILicenseSeatProfileStore NewCachedLicenseSeatProfileStore(MemoryLicenseSeatProfileStore inner, int ttl, IClock clock)
    {
        return new CachedLicenseSeatProfileStore(inner, LicenseSeatProfileCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LicenseSeatProfile> FindLicenseSeatProfiles(ILicenseSeatProfileStore store, LicenseSeatProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
