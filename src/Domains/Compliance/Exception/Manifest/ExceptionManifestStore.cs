namespace Acme.Workforce.Compliance.Exception.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception manifests by code.</summary>
public interface IExceptionManifestStore
{
    ExceptionManifest? Find(string code);
    void Save(ExceptionManifest item);
    bool Remove(string code);
    List<ExceptionManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionManifestStore(Dictionary<string, ExceptionManifest> items, List<string> order) : IExceptionManifestStore
{
    public Dictionary<string, ExceptionManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionManifest item)
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

    public List<ExceptionManifest> List()
    {
        var outValue = new List<ExceptionManifest>();
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
public sealed class CachedExceptionManifestStore(MemoryExceptionManifestStore inner, ExceptionManifestCache cache, int hits, int misses) : IExceptionManifestStore
{
    public MemoryExceptionManifestStore Inner { get; set; } = inner;
    public ExceptionManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionManifest? Find(string code)
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

    public void Save(ExceptionManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception manifests. Empty fields match all.</summary>
public sealed record ExceptionManifestQuery(string Locale, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionManifest item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ExceptionManifest> Apply(List<ExceptionManifest> items)
    {
        var matched = Collections.Filter(items, (ExceptionManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception manifest store.</summary>
public static class ExceptionManifestStore
{
    public static IExceptionManifestStore NewMemoryExceptionManifestStore()
    {
        return new MemoryExceptionManifestStore(new Dictionary<string, ExceptionManifest>(), new List<string>());
    }

    public static IExceptionManifestStore NewCachedExceptionManifestStore(MemoryExceptionManifestStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionManifestStore(inner, ExceptionManifestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ExceptionManifest> FindExceptionManifests(IExceptionManifestStore store, ExceptionManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
