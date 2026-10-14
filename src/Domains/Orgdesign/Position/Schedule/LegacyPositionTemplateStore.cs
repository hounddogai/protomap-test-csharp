namespace Acme.Workforce.Orgdesign.Position.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy position templates by code.</summary>
public interface ILegacyPositionTemplateStore
{
    LegacyPositionTemplate? Find(string code);
    void Save(LegacyPositionTemplate item);
    bool Remove(string code);
    List<LegacyPositionTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyPositionTemplateStore(Dictionary<string, LegacyPositionTemplate> items, List<string> order) : ILegacyPositionTemplateStore
{
    public Dictionary<string, LegacyPositionTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyPositionTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyPositionTemplate item)
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

    public List<LegacyPositionTemplate> List()
    {
        var outValue = new List<LegacyPositionTemplate>();
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
public sealed class CachedLegacyPositionTemplateStore(MemoryLegacyPositionTemplateStore inner, LegacyPositionTemplateCache cache, int hits, int misses) : ILegacyPositionTemplateStore
{
    public MemoryLegacyPositionTemplateStore Inner { get; set; } = inner;
    public LegacyPositionTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyPositionTemplate? Find(string code)
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

    public void Save(LegacyPositionTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyPositionTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy position templates. Empty fields match all.</summary>
public sealed record LegacyPositionTemplateQuery(string Name, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyPositionTemplate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyPositionTemplate> Apply(List<LegacyPositionTemplate> items)
    {
        var matched = Collections.Filter(items, (LegacyPositionTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy position template store.</summary>
public static class LegacyPositionTemplateStore
{
    public static ILegacyPositionTemplateStore NewMemoryLegacyPositionTemplateStore()
    {
        return new MemoryLegacyPositionTemplateStore(
            new Dictionary<string, LegacyPositionTemplate>(),
            new List<string>());
    }

    public static ILegacyPositionTemplateStore NewCachedLegacyPositionTemplateStore(MemoryLegacyPositionTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyPositionTemplateStore(
            inner,
            LegacyPositionTemplateCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LegacyPositionTemplate> FindLegacyPositionTemplates(ILegacyPositionTemplateStore store, LegacyPositionTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
