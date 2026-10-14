namespace Acme.Workforce.Compliance.Exception.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception drafts by code.</summary>
public interface IExceptionDraftStore
{
    ExceptionDraft? Find(string code);
    void Save(ExceptionDraft item);
    bool Remove(string code);
    List<ExceptionDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionDraftStore(Dictionary<string, ExceptionDraft> items, List<string> order) : IExceptionDraftStore
{
    public Dictionary<string, ExceptionDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionDraft item)
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

    public List<ExceptionDraft> List()
    {
        var outValue = new List<ExceptionDraft>();
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
public sealed class CachedExceptionDraftStore(MemoryExceptionDraftStore inner, ExceptionDraftCache cache, int hits, int misses) : IExceptionDraftStore
{
    public MemoryExceptionDraftStore Inner { get; set; } = inner;
    public ExceptionDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionDraft? Find(string code)
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

    public void Save(ExceptionDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception drafts. Empty fields match all.</summary>
public sealed record ExceptionDraftQuery(string Unit, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionDraft item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<ExceptionDraft> Apply(List<ExceptionDraft> items)
    {
        var matched = Collections.Filter(items, (ExceptionDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception draft store.</summary>
public static class ExceptionDraftStore
{
    public static IExceptionDraftStore NewMemoryExceptionDraftStore()
    {
        return new MemoryExceptionDraftStore(new Dictionary<string, ExceptionDraft>(), new List<string>());
    }

    public static IExceptionDraftStore NewCachedExceptionDraftStore(MemoryExceptionDraftStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionDraftStore(inner, ExceptionDraftCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ExceptionDraft> FindExceptionDrafts(IExceptionDraftStore store, ExceptionDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
