namespace Acme.Workforce.Talent.Nomination.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback nomination exceptions by code.</summary>
public interface IFallbackNominationExceptionStore
{
    FallbackNominationException? Find(string code);
    void Save(FallbackNominationException item);
    bool Remove(string code);
    List<FallbackNominationException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackNominationExceptionStore(Dictionary<string, FallbackNominationException> items, List<string> order) : IFallbackNominationExceptionStore
{
    public Dictionary<string, FallbackNominationException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackNominationException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackNominationException item)
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

    public List<FallbackNominationException> List()
    {
        var outValue = new List<FallbackNominationException>();
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
public sealed class CachedFallbackNominationExceptionStore(MemoryFallbackNominationExceptionStore inner, FallbackNominationExceptionCache cache, int hits, int misses) : IFallbackNominationExceptionStore
{
    public MemoryFallbackNominationExceptionStore Inner { get; set; } = inner;
    public FallbackNominationExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackNominationException? Find(string code)
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

    public void Save(FallbackNominationException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackNominationException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback nomination exceptions. Empty fields match all.</summary>
public sealed record FallbackNominationExceptionQuery(string Channel, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackNominationException item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackNominationException> Apply(List<FallbackNominationException> items)
    {
        var matched = Collections.Filter(items, (FallbackNominationException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback nomination exception store.</summary>
public static class FallbackNominationExceptionStore
{
    public static IFallbackNominationExceptionStore NewMemoryFallbackNominationExceptionStore()
    {
        return new MemoryFallbackNominationExceptionStore(
            new Dictionary<string, FallbackNominationException>(),
            new List<string>());
    }

    public static IFallbackNominationExceptionStore NewCachedFallbackNominationExceptionStore(MemoryFallbackNominationExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackNominationExceptionStore(
            inner,
            FallbackNominationExceptionCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<FallbackNominationException> FindFallbackNominationExceptions(IFallbackNominationExceptionStore store, FallbackNominationExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
