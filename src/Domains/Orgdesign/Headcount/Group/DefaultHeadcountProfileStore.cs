namespace Acme.Workforce.Orgdesign.Headcount.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default headcount profiles by code.</summary>
public interface IDefaultHeadcountProfileStore
{
    DefaultHeadcountProfile? Find(string code);
    void Save(DefaultHeadcountProfile item);
    bool Remove(string code);
    List<DefaultHeadcountProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultHeadcountProfileStore(Dictionary<string, DefaultHeadcountProfile> items, List<string> order) : IDefaultHeadcountProfileStore
{
    public Dictionary<string, DefaultHeadcountProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultHeadcountProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultHeadcountProfile item)
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

    public List<DefaultHeadcountProfile> List()
    {
        var outValue = new List<DefaultHeadcountProfile>();
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
public sealed class CachedDefaultHeadcountProfileStore(MemoryDefaultHeadcountProfileStore inner, DefaultHeadcountProfileCache cache, int hits, int misses) : IDefaultHeadcountProfileStore
{
    public MemoryDefaultHeadcountProfileStore Inner { get; set; } = inner;
    public DefaultHeadcountProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultHeadcountProfile? Find(string code)
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

    public void Save(DefaultHeadcountProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultHeadcountProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default headcount profiles. Empty fields match all.</summary>
public sealed record DefaultHeadcountProfileQuery(string Category, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultHeadcountProfile item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultHeadcountProfile> Apply(List<DefaultHeadcountProfile> items)
    {
        var matched = Collections.Filter(items, (DefaultHeadcountProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default headcount profile store.</summary>
public static class DefaultHeadcountProfileStore
{
    public static IDefaultHeadcountProfileStore NewMemoryDefaultHeadcountProfileStore()
    {
        return new MemoryDefaultHeadcountProfileStore(
            new Dictionary<string, DefaultHeadcountProfile>(),
            new List<string>());
    }

    public static IDefaultHeadcountProfileStore NewCachedDefaultHeadcountProfileStore(MemoryDefaultHeadcountProfileStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultHeadcountProfileStore(
            inner,
            DefaultHeadcountProfileCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<DefaultHeadcountProfile> FindDefaultHeadcountProfiles(IDefaultHeadcountProfileStore store, DefaultHeadcountProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
