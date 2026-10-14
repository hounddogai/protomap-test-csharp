namespace Acme.Workforce.Orgdesign.Position.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position groups by code.</summary>
public interface IPositionGroupStore
{
    PositionGroup? Find(string code);
    void Save(PositionGroup item);
    bool Remove(string code);
    List<PositionGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionGroupStore(Dictionary<string, PositionGroup> items, List<string> order) : IPositionGroupStore
{
    public Dictionary<string, PositionGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionGroup item)
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

    public List<PositionGroup> List()
    {
        var outValue = new List<PositionGroup>();
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
public sealed class CachedPositionGroupStore(MemoryPositionGroupStore inner, PositionGroupCache cache, int hits, int misses) : IPositionGroupStore
{
    public MemoryPositionGroupStore Inner { get; set; } = inner;
    public PositionGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionGroup? Find(string code)
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

    public void Save(PositionGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position groups. Empty fields match all.</summary>
public sealed record PositionGroupQuery(string Label, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(PositionGroup item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PositionGroup> Apply(List<PositionGroup> items)
    {
        var matched = Collections.Filter(items, (PositionGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position group store.</summary>
public static class PositionGroupStore
{
    public static IPositionGroupStore NewMemoryPositionGroupStore()
    {
        return new MemoryPositionGroupStore(new Dictionary<string, PositionGroup>(), new List<string>());
    }

    public static IPositionGroupStore NewCachedPositionGroupStore(MemoryPositionGroupStore inner, int ttl, IClock clock)
    {
        return new CachedPositionGroupStore(inner, PositionGroupCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PositionGroup> FindPositionGroups(IPositionGroupStore store, PositionGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
