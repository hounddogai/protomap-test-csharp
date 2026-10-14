namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default statement of work mappings by code.</summary>
public interface IDefaultStatementOfWorkMappingStore
{
    DefaultStatementOfWorkMapping? Find(string code);
    void Save(DefaultStatementOfWorkMapping item);
    bool Remove(string code);
    List<DefaultStatementOfWorkMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultStatementOfWorkMappingStore(Dictionary<string, DefaultStatementOfWorkMapping> items, List<string> order) : IDefaultStatementOfWorkMappingStore
{
    public Dictionary<string, DefaultStatementOfWorkMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultStatementOfWorkMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultStatementOfWorkMapping item)
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

    public List<DefaultStatementOfWorkMapping> List()
    {
        var outValue = new List<DefaultStatementOfWorkMapping>();
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
public sealed class CachedDefaultStatementOfWorkMappingStore(MemoryDefaultStatementOfWorkMappingStore inner, DefaultStatementOfWorkMappingCache cache, int hits, int misses) : IDefaultStatementOfWorkMappingStore
{
    public MemoryDefaultStatementOfWorkMappingStore Inner { get; set; } = inner;
    public DefaultStatementOfWorkMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultStatementOfWorkMapping? Find(string code)
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

    public void Save(DefaultStatementOfWorkMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultStatementOfWorkMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default statement of work mappings. Empty fields match all.</summary>
public sealed record DefaultStatementOfWorkMappingQuery(string Source, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultStatementOfWorkMapping item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultStatementOfWorkMapping> Apply(List<DefaultStatementOfWorkMapping> items)
    {
        var matched = Collections.Filter(items, (DefaultStatementOfWorkMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default statement of work mapping store.</summary>
public static class DefaultStatementOfWorkMappingStore
{
    public static IDefaultStatementOfWorkMappingStore NewMemoryDefaultStatementOfWorkMappingStore()
    {
        return new MemoryDefaultStatementOfWorkMappingStore(
            new Dictionary<string, DefaultStatementOfWorkMapping>(),
            new List<string>());
    }

    public static IDefaultStatementOfWorkMappingStore NewCachedDefaultStatementOfWorkMappingStore(MemoryDefaultStatementOfWorkMappingStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultStatementOfWorkMappingStore(
            inner,
            DefaultStatementOfWorkMappingCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<DefaultStatementOfWorkMapping> FindDefaultStatementOfWorkMappings(IDefaultStatementOfWorkMappingStore store, DefaultStatementOfWorkMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
