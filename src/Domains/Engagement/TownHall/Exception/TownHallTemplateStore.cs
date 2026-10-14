namespace Acme.Workforce.Engagement.TownHall.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall templates by code.</summary>
public interface ITownHallTemplateStore
{
    TownHallTemplate? Find(string code);
    void Save(TownHallTemplate item);
    bool Remove(string code);
    List<TownHallTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallTemplateStore(Dictionary<string, TownHallTemplate> items, List<string> order) : ITownHallTemplateStore
{
    public Dictionary<string, TownHallTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallTemplate item)
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

    public List<TownHallTemplate> List()
    {
        var outValue = new List<TownHallTemplate>();
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
public sealed class CachedTownHallTemplateStore(MemoryTownHallTemplateStore inner, TownHallTemplateCache cache, int hits, int misses) : ITownHallTemplateStore
{
    public MemoryTownHallTemplateStore Inner { get; set; } = inner;
    public TownHallTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallTemplate? Find(string code)
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

    public void Save(TownHallTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall templates. Empty fields match all.</summary>
public sealed record TownHallTemplateQuery(string Label, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallTemplate item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TownHallTemplate> Apply(List<TownHallTemplate> items)
    {
        var matched = Collections.Filter(items, (TownHallTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall template store.</summary>
public static class TownHallTemplateStore
{
    public static ITownHallTemplateStore NewMemoryTownHallTemplateStore()
    {
        return new MemoryTownHallTemplateStore(new Dictionary<string, TownHallTemplate>(), new List<string>());
    }

    public static ITownHallTemplateStore NewCachedTownHallTemplateStore(MemoryTownHallTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallTemplateStore(inner, TownHallTemplateCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TownHallTemplate> FindTownHallTemplates(ITownHallTemplateStore store, TownHallTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
