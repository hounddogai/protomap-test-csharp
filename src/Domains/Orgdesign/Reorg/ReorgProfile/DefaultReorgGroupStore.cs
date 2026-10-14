namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default reorg groups by code.</summary>
public interface IDefaultReorgGroupStore
{
    DefaultReorgGroup? Find(string code);
    void Save(DefaultReorgGroup item);
    bool Remove(string code);
    List<DefaultReorgGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultReorgGroupStore(Dictionary<string, DefaultReorgGroup> items, List<string> order) : IDefaultReorgGroupStore
{
    public Dictionary<string, DefaultReorgGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultReorgGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultReorgGroup item)
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

    public List<DefaultReorgGroup> List()
    {
        var outValue = new List<DefaultReorgGroup>();
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
public sealed class CachedDefaultReorgGroupStore(MemoryDefaultReorgGroupStore inner, DefaultReorgGroupCache cache, int hits, int misses) : IDefaultReorgGroupStore
{
    public MemoryDefaultReorgGroupStore Inner { get; set; } = inner;
    public DefaultReorgGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultReorgGroup? Find(string code)
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

    public void Save(DefaultReorgGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultReorgGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default reorg groups. Empty fields match all.</summary>
public sealed record DefaultReorgGroupQuery(string Label, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultReorgGroup item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultReorgGroup> Apply(List<DefaultReorgGroup> items)
    {
        var matched = Collections.Filter(items, (DefaultReorgGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default reorg group store.</summary>
public static class DefaultReorgGroupStore
{
    public static IDefaultReorgGroupStore NewMemoryDefaultReorgGroupStore()
    {
        return new MemoryDefaultReorgGroupStore(new Dictionary<string, DefaultReorgGroup>(), new List<string>());
    }

    public static IDefaultReorgGroupStore NewCachedDefaultReorgGroupStore(MemoryDefaultReorgGroupStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultReorgGroupStore(inner, DefaultReorgGroupCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<DefaultReorgGroup> FindDefaultReorgGroups(IDefaultReorgGroupStore store, DefaultReorgGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
