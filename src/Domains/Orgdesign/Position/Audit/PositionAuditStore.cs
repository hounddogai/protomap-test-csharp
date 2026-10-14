namespace Acme.Workforce.Orgdesign.Position.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position audits by code.</summary>
public interface IPositionAuditStore
{
    PositionAudit? Find(string code);
    void Save(PositionAudit item);
    bool Remove(string code);
    List<PositionAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionAuditStore(Dictionary<string, PositionAudit> items, List<string> order) : IPositionAuditStore
{
    public Dictionary<string, PositionAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionAudit item)
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

    public List<PositionAudit> List()
    {
        var outValue = new List<PositionAudit>();
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
public sealed class CachedPositionAuditStore(MemoryPositionAuditStore inner, PositionAuditCache cache, int hits, int misses) : IPositionAuditStore
{
    public MemoryPositionAuditStore Inner { get; set; } = inner;
    public PositionAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionAudit? Find(string code)
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

    public void Save(PositionAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position audits. Empty fields match all.</summary>
public sealed record PositionAuditQuery(string Title, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(PositionAudit item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<PositionAudit> Apply(List<PositionAudit> items)
    {
        var matched = Collections.Filter(items, (PositionAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position audit store.</summary>
public static class PositionAuditStore
{
    public static IPositionAuditStore NewMemoryPositionAuditStore()
    {
        return new MemoryPositionAuditStore(new Dictionary<string, PositionAudit>(), new List<string>());
    }

    public static IPositionAuditStore NewCachedPositionAuditStore(MemoryPositionAuditStore inner, int ttl, IClock clock)
    {
        return new CachedPositionAuditStore(inner, PositionAuditCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PositionAudit> FindPositionAudits(IPositionAuditStore store, PositionAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
