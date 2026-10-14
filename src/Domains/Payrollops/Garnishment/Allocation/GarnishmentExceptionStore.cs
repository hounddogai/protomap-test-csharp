namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment exceptions by code.</summary>
public interface IGarnishmentExceptionStore
{
    GarnishmentException? Find(string code);
    void Save(GarnishmentException item);
    bool Remove(string code);
    List<GarnishmentException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentExceptionStore(Dictionary<string, GarnishmentException> items, List<string> order) : IGarnishmentExceptionStore
{
    public Dictionary<string, GarnishmentException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentException item)
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

    public List<GarnishmentException> List()
    {
        var outValue = new List<GarnishmentException>();
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
public sealed class CachedGarnishmentExceptionStore(MemoryGarnishmentExceptionStore inner, GarnishmentExceptionCache cache, int hits, int misses) : IGarnishmentExceptionStore
{
    public MemoryGarnishmentExceptionStore Inner { get; set; } = inner;
    public GarnishmentExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentException? Find(string code)
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

    public void Save(GarnishmentException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment exceptions. Empty fields match all.</summary>
public sealed record GarnishmentExceptionQuery(string Title, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentException item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GarnishmentException> Apply(List<GarnishmentException> items)
    {
        var matched = Collections.Filter(items, (GarnishmentException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment exception store.</summary>
public static class GarnishmentExceptionStore
{
    public static IGarnishmentExceptionStore NewMemoryGarnishmentExceptionStore()
    {
        return new MemoryGarnishmentExceptionStore(
            new Dictionary<string, GarnishmentException>(),
            new List<string>());
    }

    public static IGarnishmentExceptionStore NewCachedGarnishmentExceptionStore(MemoryGarnishmentExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentExceptionStore(inner, GarnishmentExceptionCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<GarnishmentException> FindGarnishmentExceptions(IGarnishmentExceptionStore store, GarnishmentExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
