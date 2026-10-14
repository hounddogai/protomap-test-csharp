namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay profiles by code.</summary>
public interface IRetroPayProfileStore
{
    RetroPayProfile? Find(string code);
    void Save(RetroPayProfile item);
    bool Remove(string code);
    List<RetroPayProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayProfileStore(Dictionary<string, RetroPayProfile> items, List<string> order) : IRetroPayProfileStore
{
    public Dictionary<string, RetroPayProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayProfile item)
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

    public List<RetroPayProfile> List()
    {
        var outValue = new List<RetroPayProfile>();
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
public sealed class CachedRetroPayProfileStore(MemoryRetroPayProfileStore inner, RetroPayProfileCache cache, int hits, int misses) : IRetroPayProfileStore
{
    public MemoryRetroPayProfileStore Inner { get; set; } = inner;
    public RetroPayProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayProfile? Find(string code)
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

    public void Save(RetroPayProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay profiles. Empty fields match all.</summary>
public sealed record RetroPayProfileQuery(string Title, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayProfile item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RetroPayProfile> Apply(List<RetroPayProfile> items)
    {
        var matched = Collections.Filter(items, (RetroPayProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay profile store.</summary>
public static class RetroPayProfileStore
{
    public static IRetroPayProfileStore NewMemoryRetroPayProfileStore()
    {
        return new MemoryRetroPayProfileStore(new Dictionary<string, RetroPayProfile>(), new List<string>());
    }

    public static IRetroPayProfileStore NewCachedRetroPayProfileStore(MemoryRetroPayProfileStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayProfileStore(inner, RetroPayProfileCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RetroPayProfile> FindRetroPayProfiles(IRetroPayProfileStore store, RetroPayProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
