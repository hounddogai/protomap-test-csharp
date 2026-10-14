namespace Acme.Workforce.Engagement.Recognition.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition baselines by code.</summary>
public interface IRecognitionBaselineStore
{
    RecognitionBaseline? Find(string code);
    void Save(RecognitionBaseline item);
    bool Remove(string code);
    List<RecognitionBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionBaselineStore(Dictionary<string, RecognitionBaseline> items, List<string> order) : IRecognitionBaselineStore
{
    public Dictionary<string, RecognitionBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionBaseline item)
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

    public List<RecognitionBaseline> List()
    {
        var outValue = new List<RecognitionBaseline>();
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
public sealed class CachedRecognitionBaselineStore(MemoryRecognitionBaselineStore inner, RecognitionBaselineCache cache, int hits, int misses) : IRecognitionBaselineStore
{
    public MemoryRecognitionBaselineStore Inner { get; set; } = inner;
    public RecognitionBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionBaseline? Find(string code)
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

    public void Save(RecognitionBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition baselines. Empty fields match all.</summary>
public sealed record RecognitionBaselineQuery(string Name, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionBaseline item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RecognitionBaseline> Apply(List<RecognitionBaseline> items)
    {
        var matched = Collections.Filter(items, (RecognitionBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition baseline store.</summary>
public static class RecognitionBaselineStore
{
    public static IRecognitionBaselineStore NewMemoryRecognitionBaselineStore()
    {
        return new MemoryRecognitionBaselineStore(new Dictionary<string, RecognitionBaseline>(), new List<string>());
    }

    public static IRecognitionBaselineStore NewCachedRecognitionBaselineStore(MemoryRecognitionBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionBaselineStore(inner, RecognitionBaselineCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RecognitionBaseline> FindRecognitionBaselines(IRecognitionBaselineStore store, RecognitionBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
