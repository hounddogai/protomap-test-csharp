namespace Acme.Workforce.Engagement.Recognition.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition overrides by code.</summary>
public interface IRecognitionOverrideStore
{
    RecognitionOverride? Find(string code);
    void Save(RecognitionOverride item);
    bool Remove(string code);
    List<RecognitionOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionOverrideStore(Dictionary<string, RecognitionOverride> items, List<string> order) : IRecognitionOverrideStore
{
    public Dictionary<string, RecognitionOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionOverride item)
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

    public List<RecognitionOverride> List()
    {
        var outValue = new List<RecognitionOverride>();
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
public sealed class CachedRecognitionOverrideStore(MemoryRecognitionOverrideStore inner, RecognitionOverrideCache cache, int hits, int misses) : IRecognitionOverrideStore
{
    public MemoryRecognitionOverrideStore Inner { get; set; } = inner;
    public RecognitionOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionOverride? Find(string code)
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

    public void Save(RecognitionOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition overrides. Empty fields match all.</summary>
public sealed record RecognitionOverrideQuery(string Source, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionOverride item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RecognitionOverride> Apply(List<RecognitionOverride> items)
    {
        var matched = Collections.Filter(items, (RecognitionOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition override store.</summary>
public static class RecognitionOverrideStore
{
    public static IRecognitionOverrideStore NewMemoryRecognitionOverrideStore()
    {
        return new MemoryRecognitionOverrideStore(new Dictionary<string, RecognitionOverride>(), new List<string>());
    }

    public static IRecognitionOverrideStore NewCachedRecognitionOverrideStore(MemoryRecognitionOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionOverrideStore(inner, RecognitionOverrideCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RecognitionOverride> FindRecognitionOverrides(IRecognitionOverrideStore store, RecognitionOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
