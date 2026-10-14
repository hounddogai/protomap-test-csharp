namespace Acme.Workforce.Orgdesign.Position.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient position audits by code.</summary>
public interface ILenientPositionAuditStore
{
    LenientPositionAudit? Find(string code);
    void Save(LenientPositionAudit item);
    bool Remove(string code);
    List<LenientPositionAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientPositionAuditStore(Dictionary<string, LenientPositionAudit> items, List<string> order) : ILenientPositionAuditStore
{
    public Dictionary<string, LenientPositionAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientPositionAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientPositionAudit item)
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

    public List<LenientPositionAudit> List()
    {
        var outValue = new List<LenientPositionAudit>();
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
public sealed class CachedLenientPositionAuditStore(MemoryLenientPositionAuditStore inner, LenientPositionAuditCache cache, int hits, int misses) : ILenientPositionAuditStore
{
    public MemoryLenientPositionAuditStore Inner { get; set; } = inner;
    public LenientPositionAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientPositionAudit? Find(string code)
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

    public void Save(LenientPositionAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientPositionAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient position audits. Empty fields match all.</summary>
public sealed record LenientPositionAuditQuery(string Label, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(LenientPositionAudit item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientPositionAudit> Apply(List<LenientPositionAudit> items)
    {
        var matched = Collections.Filter(items, (LenientPositionAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient position audit store.</summary>
public static class LenientPositionAuditStore
{
    public static ILenientPositionAuditStore NewMemoryLenientPositionAuditStore()
    {
        return new MemoryLenientPositionAuditStore(
            new Dictionary<string, LenientPositionAudit>(),
            new List<string>());
    }

    public static ILenientPositionAuditStore NewCachedLenientPositionAuditStore(MemoryLenientPositionAuditStore inner, int ttl, IClock clock)
    {
        return new CachedLenientPositionAuditStore(inner, LenientPositionAuditCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LenientPositionAudit> FindLenientPositionAudits(ILenientPositionAuditStore store, LenientPositionAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
