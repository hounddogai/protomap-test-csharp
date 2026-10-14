namespace Acme.Workforce.Talent.CareerPath.PathSlot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global career path profiles by code.</summary>
public interface IGlobalCareerPathProfileStore
{
    GlobalCareerPathProfile? Find(string code);
    void Save(GlobalCareerPathProfile item);
    bool Remove(string code);
    List<GlobalCareerPathProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalCareerPathProfileStore(Dictionary<string, GlobalCareerPathProfile> items, List<string> order) : IGlobalCareerPathProfileStore
{
    public Dictionary<string, GlobalCareerPathProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalCareerPathProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalCareerPathProfile item)
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

    public List<GlobalCareerPathProfile> List()
    {
        var outValue = new List<GlobalCareerPathProfile>();
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
public sealed class CachedGlobalCareerPathProfileStore(MemoryGlobalCareerPathProfileStore inner, GlobalCareerPathProfileCache cache, int hits, int misses) : IGlobalCareerPathProfileStore
{
    public MemoryGlobalCareerPathProfileStore Inner { get; set; } = inner;
    public GlobalCareerPathProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalCareerPathProfile? Find(string code)
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

    public void Save(GlobalCareerPathProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalCareerPathProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global career path profiles. Empty fields match all.</summary>
public sealed record GlobalCareerPathProfileQuery(string Title, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalCareerPathProfile item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalCareerPathProfile> Apply(List<GlobalCareerPathProfile> items)
    {
        var matched = Collections.Filter(items, (GlobalCareerPathProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global career path profile store.</summary>
public static class GlobalCareerPathProfileStore
{
    public static IGlobalCareerPathProfileStore NewMemoryGlobalCareerPathProfileStore()
    {
        return new MemoryGlobalCareerPathProfileStore(
            new Dictionary<string, GlobalCareerPathProfile>(),
            new List<string>());
    }

    public static IGlobalCareerPathProfileStore NewCachedGlobalCareerPathProfileStore(MemoryGlobalCareerPathProfileStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalCareerPathProfileStore(
            inner,
            GlobalCareerPathProfileCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<GlobalCareerPathProfile> FindGlobalCareerPathProfiles(IGlobalCareerPathProfileStore store, GlobalCareerPathProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
