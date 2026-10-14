namespace Acme.Workforce.Orgdesign.Position.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position exceptions by code.</summary>
public interface IPositionExceptionStore
{
    PositionException? Find(string code);
    void Save(PositionException item);
    bool Remove(string code);
    List<PositionException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionExceptionStore(Dictionary<string, PositionException> items, List<string> order) : IPositionExceptionStore
{
    public Dictionary<string, PositionException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionException item)
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

    public List<PositionException> List()
    {
        var outValue = new List<PositionException>();
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
public sealed class CachedPositionExceptionStore(MemoryPositionExceptionStore inner, PositionExceptionCache cache, int hits, int misses) : IPositionExceptionStore
{
    public MemoryPositionExceptionStore Inner { get; set; } = inner;
    public PositionExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionException? Find(string code)
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

    public void Save(PositionException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position exceptions. Empty fields match all.</summary>
public sealed record PositionExceptionQuery(string Team, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(PositionException item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PositionException> Apply(List<PositionException> items)
    {
        var matched = Collections.Filter(items, (PositionException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position exception store.</summary>
public static class PositionExceptionStore
{
    public static IPositionExceptionStore NewMemoryPositionExceptionStore()
    {
        return new MemoryPositionExceptionStore(new Dictionary<string, PositionException>(), new List<string>());
    }

    public static IPositionExceptionStore NewCachedPositionExceptionStore(MemoryPositionExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedPositionExceptionStore(inner, PositionExceptionCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PositionException> FindPositionExceptions(IPositionExceptionStore store, PositionExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
