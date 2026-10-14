namespace Acme.Workforce.Workplace.AccessBadge.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge drafts by code.</summary>
public interface IAccessBadgeDraftStore
{
    AccessBadgeDraft? Find(string code);
    void Save(AccessBadgeDraft item);
    bool Remove(string code);
    List<AccessBadgeDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeDraftStore(Dictionary<string, AccessBadgeDraft> items, List<string> order) : IAccessBadgeDraftStore
{
    public Dictionary<string, AccessBadgeDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeDraft item)
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

    public List<AccessBadgeDraft> List()
    {
        var outValue = new List<AccessBadgeDraft>();
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
public sealed class CachedAccessBadgeDraftStore(MemoryAccessBadgeDraftStore inner, AccessBadgeDraftCache cache, int hits, int misses) : IAccessBadgeDraftStore
{
    public MemoryAccessBadgeDraftStore Inner { get; set; } = inner;
    public AccessBadgeDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeDraft? Find(string code)
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

    public void Save(AccessBadgeDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge drafts. Empty fields match all.</summary>
public sealed record AccessBadgeDraftQuery(string Source, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeDraft item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<AccessBadgeDraft> Apply(List<AccessBadgeDraft> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge draft store.</summary>
public static class AccessBadgeDraftStore
{
    public static IAccessBadgeDraftStore NewMemoryAccessBadgeDraftStore()
    {
        return new MemoryAccessBadgeDraftStore(new Dictionary<string, AccessBadgeDraft>(), new List<string>());
    }

    public static IAccessBadgeDraftStore NewCachedAccessBadgeDraftStore(MemoryAccessBadgeDraftStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeDraftStore(inner, AccessBadgeDraftCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeDraft> FindAccessBadgeDrafts(IAccessBadgeDraftStore store, AccessBadgeDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
