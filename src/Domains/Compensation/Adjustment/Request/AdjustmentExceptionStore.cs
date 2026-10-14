namespace Acme.Workforce.Compensation.Adjustment.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment exceptions by code.</summary>
public interface IAdjustmentExceptionStore
{
    AdjustmentException? Find(string code);
    void Save(AdjustmentException item);
    bool Remove(string code);
    List<AdjustmentException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentExceptionStore(Dictionary<string, AdjustmentException> items, List<string> order) : IAdjustmentExceptionStore
{
    public Dictionary<string, AdjustmentException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentException item)
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

    public List<AdjustmentException> List()
    {
        var outValue = new List<AdjustmentException>();
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
public sealed class CachedAdjustmentExceptionStore(MemoryAdjustmentExceptionStore inner, AdjustmentExceptionCache cache, int hits, int misses) : IAdjustmentExceptionStore
{
    public MemoryAdjustmentExceptionStore Inner { get; set; } = inner;
    public AdjustmentExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentException? Find(string code)
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

    public void Save(AdjustmentException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment exceptions. Empty fields match all.</summary>
public sealed record AdjustmentExceptionQuery(string Team, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentException item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<AdjustmentException> Apply(List<AdjustmentException> items)
    {
        var matched = Collections.Filter(items, (AdjustmentException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment exception store.</summary>
public static class AdjustmentExceptionStore
{
    public static IAdjustmentExceptionStore NewMemoryAdjustmentExceptionStore()
    {
        return new MemoryAdjustmentExceptionStore(new Dictionary<string, AdjustmentException>(), new List<string>());
    }

    public static IAdjustmentExceptionStore NewCachedAdjustmentExceptionStore(MemoryAdjustmentExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentExceptionStore(inner, AdjustmentExceptionCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AdjustmentException> FindAdjustmentExceptions(IAdjustmentExceptionStore store, AdjustmentExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
