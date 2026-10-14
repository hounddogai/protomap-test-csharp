namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending span exceptions by code.</summary>
public interface IPendingSpanExceptionStore
{
    PendingSpanException? Find(string code);
    void Save(PendingSpanException item);
    bool Remove(string code);
    List<PendingSpanException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingSpanExceptionStore(Dictionary<string, PendingSpanException> items, List<string> order) : IPendingSpanExceptionStore
{
    public Dictionary<string, PendingSpanException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingSpanException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingSpanException item)
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

    public List<PendingSpanException> List()
    {
        var outValue = new List<PendingSpanException>();
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
public sealed class CachedPendingSpanExceptionStore(MemoryPendingSpanExceptionStore inner, PendingSpanExceptionCache cache, int hits, int misses) : IPendingSpanExceptionStore
{
    public MemoryPendingSpanExceptionStore Inner { get; set; } = inner;
    public PendingSpanExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingSpanException? Find(string code)
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

    public void Save(PendingSpanException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingSpanException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending span exceptions. Empty fields match all.</summary>
public sealed record PendingSpanExceptionQuery(string Name, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(PendingSpanException item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingSpanException> Apply(List<PendingSpanException> items)
    {
        var matched = Collections.Filter(items, (PendingSpanException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending span exception store.</summary>
public static class PendingSpanExceptionStore
{
    public static IPendingSpanExceptionStore NewMemoryPendingSpanExceptionStore()
    {
        return new MemoryPendingSpanExceptionStore(
            new Dictionary<string, PendingSpanException>(),
            new List<string>());
    }

    public static IPendingSpanExceptionStore NewCachedPendingSpanExceptionStore(MemoryPendingSpanExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedPendingSpanExceptionStore(inner, PendingSpanExceptionCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PendingSpanException> FindPendingSpanExceptions(IPendingSpanExceptionStore store, PendingSpanExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
