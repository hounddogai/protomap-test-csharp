namespace Acme.Workforce.Engagement.TownHall.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall exceptions by code.</summary>
public interface ITownHallExceptionStore
{
    TownHallException? Find(string code);
    void Save(TownHallException item);
    bool Remove(string code);
    List<TownHallException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallExceptionStore(Dictionary<string, TownHallException> items, List<string> order) : ITownHallExceptionStore
{
    public Dictionary<string, TownHallException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallException item)
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

    public List<TownHallException> List()
    {
        var outValue = new List<TownHallException>();
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
public sealed class CachedTownHallExceptionStore(MemoryTownHallExceptionStore inner, TownHallExceptionCache cache, int hits, int misses) : ITownHallExceptionStore
{
    public MemoryTownHallExceptionStore Inner { get; set; } = inner;
    public TownHallExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallException? Find(string code)
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

    public void Save(TownHallException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall exceptions. Empty fields match all.</summary>
public sealed record TownHallExceptionQuery(string Unit, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallException item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TownHallException> Apply(List<TownHallException> items)
    {
        var matched = Collections.Filter(items, (TownHallException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall exception store.</summary>
public static class TownHallExceptionStore
{
    public static ITownHallExceptionStore NewMemoryTownHallExceptionStore()
    {
        return new MemoryTownHallExceptionStore(new Dictionary<string, TownHallException>(), new List<string>());
    }

    public static ITownHallExceptionStore NewCachedTownHallExceptionStore(MemoryTownHallExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallExceptionStore(inner, TownHallExceptionCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TownHallException> FindTownHallExceptions(ITownHallExceptionStore store, TownHallExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
