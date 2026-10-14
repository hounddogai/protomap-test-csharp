namespace Acme.Workforce.Engagement.Feedback.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending feedback mappings by code.</summary>
public interface IPendingFeedbackMappingStore
{
    PendingFeedbackMapping? Find(string code);
    void Save(PendingFeedbackMapping item);
    bool Remove(string code);
    List<PendingFeedbackMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingFeedbackMappingStore(Dictionary<string, PendingFeedbackMapping> items, List<string> order) : IPendingFeedbackMappingStore
{
    public Dictionary<string, PendingFeedbackMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingFeedbackMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingFeedbackMapping item)
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

    public List<PendingFeedbackMapping> List()
    {
        var outValue = new List<PendingFeedbackMapping>();
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
public sealed class CachedPendingFeedbackMappingStore(MemoryPendingFeedbackMappingStore inner, PendingFeedbackMappingCache cache, int hits, int misses) : IPendingFeedbackMappingStore
{
    public MemoryPendingFeedbackMappingStore Inner { get; set; } = inner;
    public PendingFeedbackMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingFeedbackMapping? Find(string code)
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

    public void Save(PendingFeedbackMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingFeedbackMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending feedback mappings. Empty fields match all.</summary>
public sealed record PendingFeedbackMappingQuery(string Name, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(PendingFeedbackMapping item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingFeedbackMapping> Apply(List<PendingFeedbackMapping> items)
    {
        var matched = Collections.Filter(items, (PendingFeedbackMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending feedback mapping store.</summary>
public static class PendingFeedbackMappingStore
{
    public static IPendingFeedbackMappingStore NewMemoryPendingFeedbackMappingStore()
    {
        return new MemoryPendingFeedbackMappingStore(
            new Dictionary<string, PendingFeedbackMapping>(),
            new List<string>());
    }

    public static IPendingFeedbackMappingStore NewCachedPendingFeedbackMappingStore(MemoryPendingFeedbackMappingStore inner, int ttl, IClock clock)
    {
        return new CachedPendingFeedbackMappingStore(
            inner,
            PendingFeedbackMappingCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PendingFeedbackMapping> FindPendingFeedbackMappings(IPendingFeedbackMappingStore store, PendingFeedbackMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
