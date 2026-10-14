namespace Acme.Workforce.Workplace.Desk.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default desk scores by code.</summary>
public interface IDefaultDeskScoreStore
{
    DefaultDeskScore? Find(string code);
    void Save(DefaultDeskScore item);
    bool Remove(string code);
    List<DefaultDeskScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultDeskScoreStore(Dictionary<string, DefaultDeskScore> items, List<string> order) : IDefaultDeskScoreStore
{
    public Dictionary<string, DefaultDeskScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultDeskScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultDeskScore item)
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

    public List<DefaultDeskScore> List()
    {
        var outValue = new List<DefaultDeskScore>();
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
public sealed class CachedDefaultDeskScoreStore(MemoryDefaultDeskScoreStore inner, DefaultDeskScoreCache cache, int hits, int misses) : IDefaultDeskScoreStore
{
    public MemoryDefaultDeskScoreStore Inner { get; set; } = inner;
    public DefaultDeskScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultDeskScore? Find(string code)
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

    public void Save(DefaultDeskScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultDeskScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default desk scores. Empty fields match all.</summary>
public sealed record DefaultDeskScoreQuery(string Locale, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultDeskScore item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultDeskScore> Apply(List<DefaultDeskScore> items)
    {
        var matched = Collections.Filter(items, (DefaultDeskScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default desk score store.</summary>
public static class DefaultDeskScoreStore
{
    public static IDefaultDeskScoreStore NewMemoryDefaultDeskScoreStore()
    {
        return new MemoryDefaultDeskScoreStore(new Dictionary<string, DefaultDeskScore>(), new List<string>());
    }

    public static IDefaultDeskScoreStore NewCachedDefaultDeskScoreStore(MemoryDefaultDeskScoreStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultDeskScoreStore(inner, DefaultDeskScoreCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DefaultDeskScore> FindDefaultDeskScores(IDefaultDeskScoreStore store, DefaultDeskScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
