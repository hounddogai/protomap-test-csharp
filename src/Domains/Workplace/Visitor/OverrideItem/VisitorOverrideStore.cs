namespace Acme.Workforce.Workplace.Visitor.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor overrides by code.</summary>
public interface IVisitorOverrideStore
{
    VisitorOverride? Find(string code);
    void Save(VisitorOverride item);
    bool Remove(string code);
    List<VisitorOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorOverrideStore(Dictionary<string, VisitorOverride> items, List<string> order) : IVisitorOverrideStore
{
    public Dictionary<string, VisitorOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorOverride item)
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

    public List<VisitorOverride> List()
    {
        var outValue = new List<VisitorOverride>();
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
public sealed class CachedVisitorOverrideStore(MemoryVisitorOverrideStore inner, VisitorOverrideCache cache, int hits, int misses) : IVisitorOverrideStore
{
    public MemoryVisitorOverrideStore Inner { get; set; } = inner;
    public VisitorOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorOverride? Find(string code)
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

    public void Save(VisitorOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor overrides. Empty fields match all.</summary>
public sealed record VisitorOverrideQuery(string Name, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorOverride item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorOverride> Apply(List<VisitorOverride> items)
    {
        var matched = Collections.Filter(items, (VisitorOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor override store.</summary>
public static class VisitorOverrideStore
{
    public static IVisitorOverrideStore NewMemoryVisitorOverrideStore()
    {
        return new MemoryVisitorOverrideStore(new Dictionary<string, VisitorOverride>(), new List<string>());
    }

    public static IVisitorOverrideStore NewCachedVisitorOverrideStore(MemoryVisitorOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorOverrideStore(inner, VisitorOverrideCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<VisitorOverride> FindVisitorOverrides(IVisitorOverrideStore store, VisitorOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
