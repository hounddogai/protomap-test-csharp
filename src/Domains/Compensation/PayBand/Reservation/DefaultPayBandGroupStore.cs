namespace Acme.Workforce.Compensation.PayBand.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default pay band groups by code.</summary>
public interface IDefaultPayBandGroupStore
{
    DefaultPayBandGroup? Find(string code);
    void Save(DefaultPayBandGroup item);
    bool Remove(string code);
    List<DefaultPayBandGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultPayBandGroupStore(Dictionary<string, DefaultPayBandGroup> items, List<string> order) : IDefaultPayBandGroupStore
{
    public Dictionary<string, DefaultPayBandGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultPayBandGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultPayBandGroup item)
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

    public List<DefaultPayBandGroup> List()
    {
        var outValue = new List<DefaultPayBandGroup>();
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
public sealed class CachedDefaultPayBandGroupStore(MemoryDefaultPayBandGroupStore inner, DefaultPayBandGroupCache cache, int hits, int misses) : IDefaultPayBandGroupStore
{
    public MemoryDefaultPayBandGroupStore Inner { get; set; } = inner;
    public DefaultPayBandGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultPayBandGroup? Find(string code)
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

    public void Save(DefaultPayBandGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultPayBandGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default pay band groups. Empty fields match all.</summary>
public sealed record DefaultPayBandGroupQuery(string Title, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultPayBandGroup item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultPayBandGroup> Apply(List<DefaultPayBandGroup> items)
    {
        var matched = Collections.Filter(items, (DefaultPayBandGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default pay band group store.</summary>
public static class DefaultPayBandGroupStore
{
    public static IDefaultPayBandGroupStore NewMemoryDefaultPayBandGroupStore()
    {
        return new MemoryDefaultPayBandGroupStore(new Dictionary<string, DefaultPayBandGroup>(), new List<string>());
    }

    public static IDefaultPayBandGroupStore NewCachedDefaultPayBandGroupStore(MemoryDefaultPayBandGroupStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultPayBandGroupStore(inner, DefaultPayBandGroupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<DefaultPayBandGroup> FindDefaultPayBandGroups(IDefaultPayBandGroupStore store, DefaultPayBandGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
