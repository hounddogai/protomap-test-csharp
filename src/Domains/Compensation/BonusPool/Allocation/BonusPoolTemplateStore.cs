namespace Acme.Workforce.Compensation.BonusPool.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool templates by code.</summary>
public interface IBonusPoolTemplateStore
{
    BonusPoolTemplate? Find(string code);
    void Save(BonusPoolTemplate item);
    bool Remove(string code);
    List<BonusPoolTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolTemplateStore(Dictionary<string, BonusPoolTemplate> items, List<string> order) : IBonusPoolTemplateStore
{
    public Dictionary<string, BonusPoolTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolTemplate item)
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

    public List<BonusPoolTemplate> List()
    {
        var outValue = new List<BonusPoolTemplate>();
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
public sealed class CachedBonusPoolTemplateStore(MemoryBonusPoolTemplateStore inner, BonusPoolTemplateCache cache, int hits, int misses) : IBonusPoolTemplateStore
{
    public MemoryBonusPoolTemplateStore Inner { get; set; } = inner;
    public BonusPoolTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolTemplate? Find(string code)
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

    public void Save(BonusPoolTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool templates. Empty fields match all.</summary>
public sealed record BonusPoolTemplateQuery(string Department, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolTemplate item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolTemplate> Apply(List<BonusPoolTemplate> items)
    {
        var matched = Collections.Filter(items, (BonusPoolTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool template store.</summary>
public static class BonusPoolTemplateStore
{
    public static IBonusPoolTemplateStore NewMemoryBonusPoolTemplateStore()
    {
        return new MemoryBonusPoolTemplateStore(new Dictionary<string, BonusPoolTemplate>(), new List<string>());
    }

    public static IBonusPoolTemplateStore NewCachedBonusPoolTemplateStore(MemoryBonusPoolTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolTemplateStore(inner, BonusPoolTemplateCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BonusPoolTemplate> FindBonusPoolTemplates(IBonusPoolTemplateStore store, BonusPoolTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
