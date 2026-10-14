namespace Acme.Workforce.Compliance.Exception.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception agreements by code.</summary>
public interface IExceptionAgreementStore
{
    ExceptionAgreement? Find(string code);
    void Save(ExceptionAgreement item);
    bool Remove(string code);
    List<ExceptionAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionAgreementStore(Dictionary<string, ExceptionAgreement> items, List<string> order) : IExceptionAgreementStore
{
    public Dictionary<string, ExceptionAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionAgreement item)
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

    public List<ExceptionAgreement> List()
    {
        var outValue = new List<ExceptionAgreement>();
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
public sealed class CachedExceptionAgreementStore(MemoryExceptionAgreementStore inner, ExceptionAgreementCache cache, int hits, int misses) : IExceptionAgreementStore
{
    public MemoryExceptionAgreementStore Inner { get; set; } = inner;
    public ExceptionAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionAgreement? Find(string code)
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

    public void Save(ExceptionAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception agreements. Empty fields match all.</summary>
public sealed record ExceptionAgreementQuery(string Title, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionAgreement item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ExceptionAgreement> Apply(List<ExceptionAgreement> items)
    {
        var matched = Collections.Filter(items, (ExceptionAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception agreement store.</summary>
public static class ExceptionAgreementStore
{
    public static IExceptionAgreementStore NewMemoryExceptionAgreementStore()
    {
        return new MemoryExceptionAgreementStore(new Dictionary<string, ExceptionAgreement>(), new List<string>());
    }

    public static IExceptionAgreementStore NewCachedExceptionAgreementStore(MemoryExceptionAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionAgreementStore(inner, ExceptionAgreementCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ExceptionAgreement> FindExceptionAgreements(IExceptionAgreementStore store, ExceptionAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
