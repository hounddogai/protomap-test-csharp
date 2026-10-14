namespace Acme.Workforce.Compliance.Exception.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception profiles by code.</summary>
public interface IExceptionProfileStore
{
    ExceptionProfile? Find(string code);
    void Save(ExceptionProfile item);
    bool Remove(string code);
    List<ExceptionProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionProfileStore(Dictionary<string, ExceptionProfile> items, List<string> order) : IExceptionProfileStore
{
    public Dictionary<string, ExceptionProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionProfile item)
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

    public List<ExceptionProfile> List()
    {
        var outValue = new List<ExceptionProfile>();
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
public sealed class CachedExceptionProfileStore(MemoryExceptionProfileStore inner, ExceptionProfileCache cache, int hits, int misses) : IExceptionProfileStore
{
    public MemoryExceptionProfileStore Inner { get; set; } = inner;
    public ExceptionProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionProfile? Find(string code)
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

    public void Save(ExceptionProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception profiles. Empty fields match all.</summary>
public sealed record ExceptionProfileQuery(string Channel, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionProfile item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ExceptionProfile> Apply(List<ExceptionProfile> items)
    {
        var matched = Collections.Filter(items, (ExceptionProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception profile store.</summary>
public static class ExceptionProfileStore
{
    public static IExceptionProfileStore NewMemoryExceptionProfileStore()
    {
        return new MemoryExceptionProfileStore(new Dictionary<string, ExceptionProfile>(), new List<string>());
    }

    public static IExceptionProfileStore NewCachedExceptionProfileStore(MemoryExceptionProfileStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionProfileStore(inner, ExceptionProfileCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ExceptionProfile> FindExceptionProfiles(IExceptionProfileStore store, ExceptionProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
