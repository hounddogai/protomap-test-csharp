namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount profiles by code.</summary>
public interface IHeadcountProfileStore
{
    HeadcountProfile? Find(string code);
    void Save(HeadcountProfile item);
    bool Remove(string code);
    List<HeadcountProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountProfileStore(Dictionary<string, HeadcountProfile> items, List<string> order) : IHeadcountProfileStore
{
    public Dictionary<string, HeadcountProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountProfile item)
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

    public List<HeadcountProfile> List()
    {
        var outValue = new List<HeadcountProfile>();
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
public sealed class CachedHeadcountProfileStore(MemoryHeadcountProfileStore inner, HeadcountProfileCache cache, int hits, int misses) : IHeadcountProfileStore
{
    public MemoryHeadcountProfileStore Inner { get; set; } = inner;
    public HeadcountProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountProfile? Find(string code)
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

    public void Save(HeadcountProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount profiles. Empty fields match all.</summary>
public sealed record HeadcountProfileQuery(string Locale, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountProfile item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountProfile> Apply(List<HeadcountProfile> items)
    {
        var matched = Collections.Filter(items, (HeadcountProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount profile store.</summary>
public static class HeadcountProfileStore
{
    public static IHeadcountProfileStore NewMemoryHeadcountProfileStore()
    {
        return new MemoryHeadcountProfileStore(new Dictionary<string, HeadcountProfile>(), new List<string>());
    }

    public static IHeadcountProfileStore NewCachedHeadcountProfileStore(MemoryHeadcountProfileStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountProfileStore(inner, HeadcountProfileCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<HeadcountProfile> FindHeadcountProfiles(IHeadcountProfileStore store, HeadcountProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
