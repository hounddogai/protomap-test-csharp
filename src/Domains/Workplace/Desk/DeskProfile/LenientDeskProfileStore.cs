namespace Acme.Workforce.Workplace.Desk.DeskProfile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient desk profiles by code.</summary>
public interface ILenientDeskProfileStore
{
    LenientDeskProfile? Find(string code);
    void Save(LenientDeskProfile item);
    bool Remove(string code);
    List<LenientDeskProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientDeskProfileStore(Dictionary<string, LenientDeskProfile> items, List<string> order) : ILenientDeskProfileStore
{
    public Dictionary<string, LenientDeskProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientDeskProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientDeskProfile item)
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

    public List<LenientDeskProfile> List()
    {
        var outValue = new List<LenientDeskProfile>();
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
public sealed class CachedLenientDeskProfileStore(MemoryLenientDeskProfileStore inner, LenientDeskProfileCache cache, int hits, int misses) : ILenientDeskProfileStore
{
    public MemoryLenientDeskProfileStore Inner { get; set; } = inner;
    public LenientDeskProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientDeskProfile? Find(string code)
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

    public void Save(LenientDeskProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientDeskProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient desk profiles. Empty fields match all.</summary>
public sealed record LenientDeskProfileQuery(string Name, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(LenientDeskProfile item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientDeskProfile> Apply(List<LenientDeskProfile> items)
    {
        var matched = Collections.Filter(items, (LenientDeskProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient desk profile store.</summary>
public static class LenientDeskProfileStore
{
    public static ILenientDeskProfileStore NewMemoryLenientDeskProfileStore()
    {
        return new MemoryLenientDeskProfileStore(new Dictionary<string, LenientDeskProfile>(), new List<string>());
    }

    public static ILenientDeskProfileStore NewCachedLenientDeskProfileStore(MemoryLenientDeskProfileStore inner, int ttl, IClock clock)
    {
        return new CachedLenientDeskProfileStore(inner, LenientDeskProfileCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LenientDeskProfile> FindLenientDeskProfiles(ILenientDeskProfileStore store, LenientDeskProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
