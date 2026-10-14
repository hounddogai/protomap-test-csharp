namespace Acme.Workforce.Engagement.Recognition.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default recognition thresholds by code.</summary>
public interface IDefaultRecognitionThresholdStore
{
    DefaultRecognitionThreshold? Find(string code);
    void Save(DefaultRecognitionThreshold item);
    bool Remove(string code);
    List<DefaultRecognitionThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultRecognitionThresholdStore(Dictionary<string, DefaultRecognitionThreshold> items, List<string> order) : IDefaultRecognitionThresholdStore
{
    public Dictionary<string, DefaultRecognitionThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultRecognitionThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultRecognitionThreshold item)
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

    public List<DefaultRecognitionThreshold> List()
    {
        var outValue = new List<DefaultRecognitionThreshold>();
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
public sealed class CachedDefaultRecognitionThresholdStore(MemoryDefaultRecognitionThresholdStore inner, DefaultRecognitionThresholdCache cache, int hits, int misses) : IDefaultRecognitionThresholdStore
{
    public MemoryDefaultRecognitionThresholdStore Inner { get; set; } = inner;
    public DefaultRecognitionThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultRecognitionThreshold? Find(string code)
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

    public void Save(DefaultRecognitionThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultRecognitionThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default recognition thresholds. Empty fields match all.</summary>
public sealed record DefaultRecognitionThresholdQuery(string Channel, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultRecognitionThreshold item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<DefaultRecognitionThreshold> Apply(List<DefaultRecognitionThreshold> items)
    {
        var matched = Collections.Filter(items, (DefaultRecognitionThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default recognition threshold store.</summary>
public static class DefaultRecognitionThresholdStore
{
    public static IDefaultRecognitionThresholdStore NewMemoryDefaultRecognitionThresholdStore()
    {
        return new MemoryDefaultRecognitionThresholdStore(
            new Dictionary<string, DefaultRecognitionThreshold>(),
            new List<string>());
    }

    public static IDefaultRecognitionThresholdStore NewCachedDefaultRecognitionThresholdStore(MemoryDefaultRecognitionThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultRecognitionThresholdStore(
            inner,
            DefaultRecognitionThresholdCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<DefaultRecognitionThreshold> FindDefaultRecognitionThresholds(IDefaultRecognitionThresholdStore store, DefaultRecognitionThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
