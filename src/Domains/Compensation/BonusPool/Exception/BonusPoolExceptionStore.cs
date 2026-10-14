namespace Acme.Workforce.Compensation.BonusPool.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool exceptions by code.</summary>
public interface IBonusPoolExceptionStore
{
    BonusPoolException? Find(string code);
    void Save(BonusPoolException item);
    bool Remove(string code);
    List<BonusPoolException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolExceptionStore(Dictionary<string, BonusPoolException> items, List<string> order) : IBonusPoolExceptionStore
{
    public Dictionary<string, BonusPoolException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolException item)
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

    public List<BonusPoolException> List()
    {
        var outValue = new List<BonusPoolException>();
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
public sealed class CachedBonusPoolExceptionStore(MemoryBonusPoolExceptionStore inner, BonusPoolExceptionCache cache, int hits, int misses) : IBonusPoolExceptionStore
{
    public MemoryBonusPoolExceptionStore Inner { get; set; } = inner;
    public BonusPoolExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolException? Find(string code)
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

    public void Save(BonusPoolException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool exceptions. Empty fields match all.</summary>
public sealed record BonusPoolExceptionQuery(string Label, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolException item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolException> Apply(List<BonusPoolException> items)
    {
        var matched = Collections.Filter(items, (BonusPoolException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool exception store.</summary>
public static class BonusPoolExceptionStore
{
    public static IBonusPoolExceptionStore NewMemoryBonusPoolExceptionStore()
    {
        return new MemoryBonusPoolExceptionStore(new Dictionary<string, BonusPoolException>(), new List<string>());
    }

    public static IBonusPoolExceptionStore NewCachedBonusPoolExceptionStore(MemoryBonusPoolExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolExceptionStore(inner, BonusPoolExceptionCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BonusPoolException> FindBonusPoolExceptions(IBonusPoolExceptionStore store, BonusPoolExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
