namespace Acme.Workforce.Orgdesign.Headcount.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount templates by code.</summary>
public interface IHeadcountTemplateStore
{
    HeadcountTemplate? Find(string code);
    void Save(HeadcountTemplate item);
    bool Remove(string code);
    List<HeadcountTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountTemplateStore(Dictionary<string, HeadcountTemplate> items, List<string> order) : IHeadcountTemplateStore
{
    public Dictionary<string, HeadcountTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountTemplate item)
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

    public List<HeadcountTemplate> List()
    {
        var outValue = new List<HeadcountTemplate>();
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
public sealed class CachedHeadcountTemplateStore(MemoryHeadcountTemplateStore inner, HeadcountTemplateCache cache, int hits, int misses) : IHeadcountTemplateStore
{
    public MemoryHeadcountTemplateStore Inner { get; set; } = inner;
    public HeadcountTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountTemplate? Find(string code)
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

    public void Save(HeadcountTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount templates. Empty fields match all.</summary>
public sealed record HeadcountTemplateQuery(string Team, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountTemplate item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountTemplate> Apply(List<HeadcountTemplate> items)
    {
        var matched = Collections.Filter(items, (HeadcountTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount template store.</summary>
public static class HeadcountTemplateStore
{
    public static IHeadcountTemplateStore NewMemoryHeadcountTemplateStore()
    {
        return new MemoryHeadcountTemplateStore(new Dictionary<string, HeadcountTemplate>(), new List<string>());
    }

    public static IHeadcountTemplateStore NewCachedHeadcountTemplateStore(MemoryHeadcountTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountTemplateStore(inner, HeadcountTemplateCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<HeadcountTemplate> FindHeadcountTemplates(IHeadcountTemplateStore store, HeadcountTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
