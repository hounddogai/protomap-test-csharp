namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor manifests by code.</summary>
public interface IVisitorManifestStore
{
    VisitorManifest? Find(string code);
    void Save(VisitorManifest item);
    bool Remove(string code);
    List<VisitorManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorManifestStore(Dictionary<string, VisitorManifest> items, List<string> order) : IVisitorManifestStore
{
    public Dictionary<string, VisitorManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorManifest item)
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

    public List<VisitorManifest> List()
    {
        var outValue = new List<VisitorManifest>();
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
public sealed class CachedVisitorManifestStore(MemoryVisitorManifestStore inner, VisitorManifestCache cache, int hits, int misses) : IVisitorManifestStore
{
    public MemoryVisitorManifestStore Inner { get; set; } = inner;
    public VisitorManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorManifest? Find(string code)
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

    public void Save(VisitorManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor manifests. Empty fields match all.</summary>
public sealed record VisitorManifestQuery(string Locale, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorManifest item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorManifest> Apply(List<VisitorManifest> items)
    {
        var matched = Collections.Filter(items, (VisitorManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor manifest store.</summary>
public static class VisitorManifestStore
{
    public static IVisitorManifestStore NewMemoryVisitorManifestStore()
    {
        return new MemoryVisitorManifestStore(new Dictionary<string, VisitorManifest>(), new List<string>());
    }

    public static IVisitorManifestStore NewCachedVisitorManifestStore(MemoryVisitorManifestStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorManifestStore(inner, VisitorManifestCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<VisitorManifest> FindVisitorManifests(IVisitorManifestStore store, VisitorManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
