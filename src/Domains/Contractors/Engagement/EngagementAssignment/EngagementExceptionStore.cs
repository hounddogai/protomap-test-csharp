namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps engagement exceptions by code.</summary>
public interface IEngagementExceptionStore
{
    EngagementException? Find(string code);
    void Save(EngagementException item);
    bool Remove(string code);
    List<EngagementException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEngagementExceptionStore(Dictionary<string, EngagementException> items, List<string> order) : IEngagementExceptionStore
{
    public Dictionary<string, EngagementException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EngagementException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EngagementException item)
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

    public List<EngagementException> List()
    {
        var outValue = new List<EngagementException>();
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
public sealed class CachedEngagementExceptionStore(MemoryEngagementExceptionStore inner, EngagementExceptionCache cache, int hits, int misses) : IEngagementExceptionStore
{
    public MemoryEngagementExceptionStore Inner { get; set; } = inner;
    public EngagementExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EngagementException? Find(string code)
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

    public void Save(EngagementException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EngagementException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects engagement exceptions. Empty fields match all.</summary>
public sealed record EngagementExceptionQuery(string Locale, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(EngagementException item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<EngagementException> Apply(List<EngagementException> items)
    {
        var matched = Collections.Filter(items, (EngagementException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of engagement exception store.</summary>
public static class EngagementExceptionStore
{
    public static IEngagementExceptionStore NewMemoryEngagementExceptionStore()
    {
        return new MemoryEngagementExceptionStore(new Dictionary<string, EngagementException>(), new List<string>());
    }

    public static IEngagementExceptionStore NewCachedEngagementExceptionStore(MemoryEngagementExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedEngagementExceptionStore(inner, EngagementExceptionCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<EngagementException> FindEngagementExceptions(IEngagementExceptionStore store, EngagementExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
