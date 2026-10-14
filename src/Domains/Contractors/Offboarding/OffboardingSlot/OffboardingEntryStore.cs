namespace Acme.Workforce.Contractors.Offboarding.OffboardingSlot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding entries by code.</summary>
public interface IOffboardingEntryStore
{
    OffboardingEntry? Find(string code);
    void Save(OffboardingEntry item);
    bool Remove(string code);
    List<OffboardingEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingEntryStore(Dictionary<string, OffboardingEntry> items, List<string> order) : IOffboardingEntryStore
{
    public Dictionary<string, OffboardingEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingEntry item)
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

    public List<OffboardingEntry> List()
    {
        var outValue = new List<OffboardingEntry>();
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
public sealed class CachedOffboardingEntryStore(MemoryOffboardingEntryStore inner, OffboardingEntryCache cache, int hits, int misses) : IOffboardingEntryStore
{
    public MemoryOffboardingEntryStore Inner { get; set; } = inner;
    public OffboardingEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingEntry? Find(string code)
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

    public void Save(OffboardingEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding entries. Empty fields match all.</summary>
public sealed record OffboardingEntryQuery(string Segment, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingEntry item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OffboardingEntry> Apply(List<OffboardingEntry> items)
    {
        var matched = Collections.Filter(items, (OffboardingEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding entry store.</summary>
public static class OffboardingEntryStore
{
    public static IOffboardingEntryStore NewMemoryOffboardingEntryStore()
    {
        return new MemoryOffboardingEntryStore(new Dictionary<string, OffboardingEntry>(), new List<string>());
    }

    public static IOffboardingEntryStore NewCachedOffboardingEntryStore(MemoryOffboardingEntryStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingEntryStore(inner, OffboardingEntryCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<OffboardingEntry> FindOffboardingEntries(IOffboardingEntryStore store, OffboardingEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
