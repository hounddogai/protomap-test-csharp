namespace Acme.Workforce.Absence.Holiday.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped holiday manifests by code.</summary>
public interface ICappedHolidayManifestStore
{
    CappedHolidayManifest? Find(string code);
    void Save(CappedHolidayManifest item);
    bool Remove(string code);
    List<CappedHolidayManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedHolidayManifestStore(Dictionary<string, CappedHolidayManifest> items, List<string> order) : ICappedHolidayManifestStore
{
    public Dictionary<string, CappedHolidayManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedHolidayManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedHolidayManifest item)
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

    public List<CappedHolidayManifest> List()
    {
        var outValue = new List<CappedHolidayManifest>();
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
public sealed class CachedCappedHolidayManifestStore(MemoryCappedHolidayManifestStore inner, CappedHolidayManifestCache cache, int hits, int misses) : ICappedHolidayManifestStore
{
    public MemoryCappedHolidayManifestStore Inner { get; set; } = inner;
    public CappedHolidayManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedHolidayManifest? Find(string code)
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

    public void Save(CappedHolidayManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedHolidayManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped holiday manifests. Empty fields match all.</summary>
public sealed record CappedHolidayManifestQuery(string Segment, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(CappedHolidayManifest item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedHolidayManifest> Apply(List<CappedHolidayManifest> items)
    {
        var matched = Collections.Filter(items, (CappedHolidayManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped holiday manifest store.</summary>
public static class CappedHolidayManifestStore
{
    public static ICappedHolidayManifestStore NewMemoryCappedHolidayManifestStore()
    {
        return new MemoryCappedHolidayManifestStore(
            new Dictionary<string, CappedHolidayManifest>(),
            new List<string>());
    }

    public static ICappedHolidayManifestStore NewCachedCappedHolidayManifestStore(MemoryCappedHolidayManifestStore inner, int ttl, IClock clock)
    {
        return new CachedCappedHolidayManifestStore(inner, CappedHolidayManifestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CappedHolidayManifest> FindCappedHolidayManifests(ICappedHolidayManifestStore store, CappedHolidayManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
