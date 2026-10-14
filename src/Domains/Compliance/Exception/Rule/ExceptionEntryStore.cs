namespace Acme.Workforce.Compliance.Exception.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception entries by code.</summary>
public interface IExceptionEntryStore
{
    ExceptionEntry? Find(string code);
    void Save(ExceptionEntry item);
    bool Remove(string code);
    List<ExceptionEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionEntryStore(Dictionary<string, ExceptionEntry> items, List<string> order) : IExceptionEntryStore
{
    public Dictionary<string, ExceptionEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionEntry item)
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

    public List<ExceptionEntry> List()
    {
        var outValue = new List<ExceptionEntry>();
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
public sealed class CachedExceptionEntryStore(MemoryExceptionEntryStore inner, ExceptionEntryCache cache, int hits, int misses) : IExceptionEntryStore
{
    public MemoryExceptionEntryStore Inner { get; set; } = inner;
    public ExceptionEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionEntry? Find(string code)
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

    public void Save(ExceptionEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception entries. Empty fields match all.</summary>
public sealed record ExceptionEntryQuery(string Source, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionEntry item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ExceptionEntry> Apply(List<ExceptionEntry> items)
    {
        var matched = Collections.Filter(items, (ExceptionEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception entry store.</summary>
public static class ExceptionEntryStore
{
    public static IExceptionEntryStore NewMemoryExceptionEntryStore()
    {
        return new MemoryExceptionEntryStore(new Dictionary<string, ExceptionEntry>(), new List<string>());
    }

    public static IExceptionEntryStore NewCachedExceptionEntryStore(MemoryExceptionEntryStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionEntryStore(inner, ExceptionEntryCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ExceptionEntry> FindExceptionEntries(IExceptionEntryStore store, ExceptionEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
