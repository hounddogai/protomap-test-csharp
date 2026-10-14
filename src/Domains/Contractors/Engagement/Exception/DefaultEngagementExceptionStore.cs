namespace Acme.Workforce.Contractors.Engagement.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default engagement exceptions by code.</summary>
public interface IDefaultEngagementExceptionStore
{
    DefaultEngagementException? Find(string code);
    void Save(DefaultEngagementException item);
    bool Remove(string code);
    List<DefaultEngagementException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultEngagementExceptionStore(Dictionary<string, DefaultEngagementException> items, List<string> order) : IDefaultEngagementExceptionStore
{
    public Dictionary<string, DefaultEngagementException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultEngagementException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultEngagementException item)
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

    public List<DefaultEngagementException> List()
    {
        var outValue = new List<DefaultEngagementException>();
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
public sealed class CachedDefaultEngagementExceptionStore(MemoryDefaultEngagementExceptionStore inner, DefaultEngagementExceptionCache cache, int hits, int misses) : IDefaultEngagementExceptionStore
{
    public MemoryDefaultEngagementExceptionStore Inner { get; set; } = inner;
    public DefaultEngagementExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultEngagementException? Find(string code)
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

    public void Save(DefaultEngagementException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultEngagementException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default engagement exceptions. Empty fields match all.</summary>
public sealed record DefaultEngagementExceptionQuery(string Name, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultEngagementException item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultEngagementException> Apply(List<DefaultEngagementException> items)
    {
        var matched = Collections.Filter(items, (DefaultEngagementException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default engagement exception store.</summary>
public static class DefaultEngagementExceptionStore
{
    public static IDefaultEngagementExceptionStore NewMemoryDefaultEngagementExceptionStore()
    {
        return new MemoryDefaultEngagementExceptionStore(
            new Dictionary<string, DefaultEngagementException>(),
            new List<string>());
    }

    public static IDefaultEngagementExceptionStore NewCachedDefaultEngagementExceptionStore(MemoryDefaultEngagementExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultEngagementExceptionStore(
            inner,
            DefaultEngagementExceptionCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<DefaultEngagementException> FindDefaultEngagementExceptions(IDefaultEngagementExceptionStore store, DefaultEngagementExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
