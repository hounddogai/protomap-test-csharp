namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending visitor profiles by code.</summary>
public interface IPendingVisitorProfileStore
{
    PendingVisitorProfile? Find(string code);
    void Save(PendingVisitorProfile item);
    bool Remove(string code);
    List<PendingVisitorProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingVisitorProfileStore(Dictionary<string, PendingVisitorProfile> items, List<string> order) : IPendingVisitorProfileStore
{
    public Dictionary<string, PendingVisitorProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingVisitorProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingVisitorProfile item)
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

    public List<PendingVisitorProfile> List()
    {
        var outValue = new List<PendingVisitorProfile>();
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
public sealed class CachedPendingVisitorProfileStore(MemoryPendingVisitorProfileStore inner, PendingVisitorProfileCache cache, int hits, int misses) : IPendingVisitorProfileStore
{
    public MemoryPendingVisitorProfileStore Inner { get; set; } = inner;
    public PendingVisitorProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingVisitorProfile? Find(string code)
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

    public void Save(PendingVisitorProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingVisitorProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending visitor profiles. Empty fields match all.</summary>
public sealed record PendingVisitorProfileQuery(string Title, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(PendingVisitorProfile item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingVisitorProfile> Apply(List<PendingVisitorProfile> items)
    {
        var matched = Collections.Filter(items, (PendingVisitorProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending visitor profile store.</summary>
public static class PendingVisitorProfileStore
{
    public static IPendingVisitorProfileStore NewMemoryPendingVisitorProfileStore()
    {
        return new MemoryPendingVisitorProfileStore(
            new Dictionary<string, PendingVisitorProfile>(),
            new List<string>());
    }

    public static IPendingVisitorProfileStore NewCachedPendingVisitorProfileStore(MemoryPendingVisitorProfileStore inner, int ttl, IClock clock)
    {
        return new CachedPendingVisitorProfileStore(inner, PendingVisitorProfileCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PendingVisitorProfile> FindPendingVisitorProfiles(IPendingVisitorProfileStore store, PendingVisitorProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
