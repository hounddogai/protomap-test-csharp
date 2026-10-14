namespace Acme.Workforce.Workplace.Booking.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default booking scores by code.</summary>
public interface IDefaultBookingScoreStore
{
    DefaultBookingScore? Find(string code);
    void Save(DefaultBookingScore item);
    bool Remove(string code);
    List<DefaultBookingScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultBookingScoreStore(Dictionary<string, DefaultBookingScore> items, List<string> order) : IDefaultBookingScoreStore
{
    public Dictionary<string, DefaultBookingScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultBookingScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultBookingScore item)
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

    public List<DefaultBookingScore> List()
    {
        var outValue = new List<DefaultBookingScore>();
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
public sealed class CachedDefaultBookingScoreStore(MemoryDefaultBookingScoreStore inner, DefaultBookingScoreCache cache, int hits, int misses) : IDefaultBookingScoreStore
{
    public MemoryDefaultBookingScoreStore Inner { get; set; } = inner;
    public DefaultBookingScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultBookingScore? Find(string code)
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

    public void Save(DefaultBookingScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultBookingScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default booking scores. Empty fields match all.</summary>
public sealed record DefaultBookingScoreQuery(string Locale, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultBookingScore item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultBookingScore> Apply(List<DefaultBookingScore> items)
    {
        var matched = Collections.Filter(items, (DefaultBookingScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default booking score store.</summary>
public static class DefaultBookingScoreStore
{
    public static IDefaultBookingScoreStore NewMemoryDefaultBookingScoreStore()
    {
        return new MemoryDefaultBookingScoreStore(new Dictionary<string, DefaultBookingScore>(), new List<string>());
    }

    public static IDefaultBookingScoreStore NewCachedDefaultBookingScoreStore(MemoryDefaultBookingScoreStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultBookingScoreStore(inner, DefaultBookingScoreCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DefaultBookingScore> FindDefaultBookingScores(IDefaultBookingScoreStore store, DefaultBookingScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
