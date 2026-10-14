namespace Acme.Workforce.Engagement.Recognition.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient recognition entries by code.</summary>
public interface ILenientRecognitionEntryStore
{
    LenientRecognitionEntry? Find(string code);
    void Save(LenientRecognitionEntry item);
    bool Remove(string code);
    List<LenientRecognitionEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientRecognitionEntryStore(Dictionary<string, LenientRecognitionEntry> items, List<string> order) : ILenientRecognitionEntryStore
{
    public Dictionary<string, LenientRecognitionEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientRecognitionEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientRecognitionEntry item)
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

    public List<LenientRecognitionEntry> List()
    {
        var outValue = new List<LenientRecognitionEntry>();
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
public sealed class CachedLenientRecognitionEntryStore(MemoryLenientRecognitionEntryStore inner, LenientRecognitionEntryCache cache, int hits, int misses) : ILenientRecognitionEntryStore
{
    public MemoryLenientRecognitionEntryStore Inner { get; set; } = inner;
    public LenientRecognitionEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientRecognitionEntry? Find(string code)
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

    public void Save(LenientRecognitionEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientRecognitionEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient recognition entries. Empty fields match all.</summary>
public sealed record LenientRecognitionEntryQuery(string Channel, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(LenientRecognitionEntry item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<LenientRecognitionEntry> Apply(List<LenientRecognitionEntry> items)
    {
        var matched = Collections.Filter(items, (LenientRecognitionEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient recognition entry store.</summary>
public static class LenientRecognitionEntryStore
{
    public static ILenientRecognitionEntryStore NewMemoryLenientRecognitionEntryStore()
    {
        return new MemoryLenientRecognitionEntryStore(
            new Dictionary<string, LenientRecognitionEntry>(),
            new List<string>());
    }

    public static ILenientRecognitionEntryStore NewCachedLenientRecognitionEntryStore(MemoryLenientRecognitionEntryStore inner, int ttl, IClock clock)
    {
        return new CachedLenientRecognitionEntryStore(
            inner,
            LenientRecognitionEntryCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LenientRecognitionEntry> FindLenientRecognitionEntries(ILenientRecognitionEntryStore store, LenientRecognitionEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
