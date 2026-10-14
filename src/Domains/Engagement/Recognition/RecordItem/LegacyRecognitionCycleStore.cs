namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy recognition cycles by code.</summary>
public interface ILegacyRecognitionCycleStore
{
    LegacyRecognitionCycle? Find(string code);
    void Save(LegacyRecognitionCycle item);
    bool Remove(string code);
    List<LegacyRecognitionCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyRecognitionCycleStore(Dictionary<string, LegacyRecognitionCycle> items, List<string> order) : ILegacyRecognitionCycleStore
{
    public Dictionary<string, LegacyRecognitionCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyRecognitionCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyRecognitionCycle item)
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

    public List<LegacyRecognitionCycle> List()
    {
        var outValue = new List<LegacyRecognitionCycle>();
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
public sealed class CachedLegacyRecognitionCycleStore(MemoryLegacyRecognitionCycleStore inner, LegacyRecognitionCycleCache cache, int hits, int misses) : ILegacyRecognitionCycleStore
{
    public MemoryLegacyRecognitionCycleStore Inner { get; set; } = inner;
    public LegacyRecognitionCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyRecognitionCycle? Find(string code)
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

    public void Save(LegacyRecognitionCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyRecognitionCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy recognition cycles. Empty fields match all.</summary>
public sealed record LegacyRecognitionCycleQuery(string Title, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyRecognitionCycle item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyRecognitionCycle> Apply(List<LegacyRecognitionCycle> items)
    {
        var matched = Collections.Filter(items, (LegacyRecognitionCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy recognition cycle store.</summary>
public static class LegacyRecognitionCycleStore
{
    public static ILegacyRecognitionCycleStore NewMemoryLegacyRecognitionCycleStore()
    {
        return new MemoryLegacyRecognitionCycleStore(
            new Dictionary<string, LegacyRecognitionCycle>(),
            new List<string>());
    }

    public static ILegacyRecognitionCycleStore NewCachedLegacyRecognitionCycleStore(MemoryLegacyRecognitionCycleStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyRecognitionCycleStore(
            inner,
            LegacyRecognitionCycleCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LegacyRecognitionCycle> FindLegacyRecognitionCycles(ILegacyRecognitionCycleStore store, LegacyRecognitionCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
