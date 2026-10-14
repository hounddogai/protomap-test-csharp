namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional pay run templates by code.</summary>
public interface IRegionalPayRunTemplateStore
{
    RegionalPayRunTemplate? Find(string code);
    void Save(RegionalPayRunTemplate item);
    bool Remove(string code);
    List<RegionalPayRunTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalPayRunTemplateStore(Dictionary<string, RegionalPayRunTemplate> items, List<string> order) : IRegionalPayRunTemplateStore
{
    public Dictionary<string, RegionalPayRunTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalPayRunTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalPayRunTemplate item)
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

    public List<RegionalPayRunTemplate> List()
    {
        var outValue = new List<RegionalPayRunTemplate>();
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
public sealed class CachedRegionalPayRunTemplateStore(MemoryRegionalPayRunTemplateStore inner, RegionalPayRunTemplateCache cache, int hits, int misses) : IRegionalPayRunTemplateStore
{
    public MemoryRegionalPayRunTemplateStore Inner { get; set; } = inner;
    public RegionalPayRunTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalPayRunTemplate? Find(string code)
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

    public void Save(RegionalPayRunTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalPayRunTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional pay run templates. Empty fields match all.</summary>
public sealed record RegionalPayRunTemplateQuery(string Channel, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalPayRunTemplate item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<RegionalPayRunTemplate> Apply(List<RegionalPayRunTemplate> items)
    {
        var matched = Collections.Filter(items, (RegionalPayRunTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional pay run template store.</summary>
public static class RegionalPayRunTemplateStore
{
    public static IRegionalPayRunTemplateStore NewMemoryRegionalPayRunTemplateStore()
    {
        return new MemoryRegionalPayRunTemplateStore(
            new Dictionary<string, RegionalPayRunTemplate>(),
            new List<string>());
    }

    public static IRegionalPayRunTemplateStore NewCachedRegionalPayRunTemplateStore(MemoryRegionalPayRunTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalPayRunTemplateStore(
            inner,
            RegionalPayRunTemplateCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RegionalPayRunTemplate> FindRegionalPayRunTemplates(IRegionalPayRunTemplateStore store, RegionalPayRunTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
