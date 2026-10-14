namespace Acme.Workforce.Assets.Refresh.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient refresh profiles by code.</summary>
public interface ILenientRefreshProfileStore
{
    LenientRefreshProfile? Find(string code);
    void Save(LenientRefreshProfile item);
    bool Remove(string code);
    List<LenientRefreshProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientRefreshProfileStore(Dictionary<string, LenientRefreshProfile> items, List<string> order) : ILenientRefreshProfileStore
{
    public Dictionary<string, LenientRefreshProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientRefreshProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientRefreshProfile item)
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

    public List<LenientRefreshProfile> List()
    {
        var outValue = new List<LenientRefreshProfile>();
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
public sealed class CachedLenientRefreshProfileStore(MemoryLenientRefreshProfileStore inner, LenientRefreshProfileCache cache, int hits, int misses) : ILenientRefreshProfileStore
{
    public MemoryLenientRefreshProfileStore Inner { get; set; } = inner;
    public LenientRefreshProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientRefreshProfile? Find(string code)
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

    public void Save(LenientRefreshProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientRefreshProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient refresh profiles. Empty fields match all.</summary>
public sealed record LenientRefreshProfileQuery(string Team, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(LenientRefreshProfile item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientRefreshProfile> Apply(List<LenientRefreshProfile> items)
    {
        var matched = Collections.Filter(items, (LenientRefreshProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient refresh profile store.</summary>
public static class LenientRefreshProfileStore
{
    public static ILenientRefreshProfileStore NewMemoryLenientRefreshProfileStore()
    {
        return new MemoryLenientRefreshProfileStore(
            new Dictionary<string, LenientRefreshProfile>(),
            new List<string>());
    }

    public static ILenientRefreshProfileStore NewCachedLenientRefreshProfileStore(MemoryLenientRefreshProfileStore inner, int ttl, IClock clock)
    {
        return new CachedLenientRefreshProfileStore(inner, LenientRefreshProfileCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LenientRefreshProfile> FindLenientRefreshProfiles(ILenientRefreshProfileStore store, LenientRefreshProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
