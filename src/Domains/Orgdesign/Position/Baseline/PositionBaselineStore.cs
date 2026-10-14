namespace Acme.Workforce.Orgdesign.Position.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position baselines by code.</summary>
public interface IPositionBaselineStore
{
    PositionBaseline? Find(string code);
    void Save(PositionBaseline item);
    bool Remove(string code);
    List<PositionBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionBaselineStore(Dictionary<string, PositionBaseline> items, List<string> order) : IPositionBaselineStore
{
    public Dictionary<string, PositionBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionBaseline item)
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

    public List<PositionBaseline> List()
    {
        var outValue = new List<PositionBaseline>();
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
public sealed class CachedPositionBaselineStore(MemoryPositionBaselineStore inner, PositionBaselineCache cache, int hits, int misses) : IPositionBaselineStore
{
    public MemoryPositionBaselineStore Inner { get; set; } = inner;
    public PositionBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionBaseline? Find(string code)
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

    public void Save(PositionBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position baselines. Empty fields match all.</summary>
public sealed record PositionBaselineQuery(string Label, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PositionBaseline item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<PositionBaseline> Apply(List<PositionBaseline> items)
    {
        var matched = Collections.Filter(items, (PositionBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position baseline store.</summary>
public static class PositionBaselineStore
{
    public static IPositionBaselineStore NewMemoryPositionBaselineStore()
    {
        return new MemoryPositionBaselineStore(new Dictionary<string, PositionBaseline>(), new List<string>());
    }

    public static IPositionBaselineStore NewCachedPositionBaselineStore(MemoryPositionBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedPositionBaselineStore(inner, PositionBaselineCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PositionBaseline> FindPositionBaselines(IPositionBaselineStore store, PositionBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
