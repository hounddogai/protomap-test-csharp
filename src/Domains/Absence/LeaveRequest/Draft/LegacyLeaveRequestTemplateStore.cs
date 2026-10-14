namespace Acme.Workforce.Absence.LeaveRequest.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy leave request templates by code.</summary>
public interface ILegacyLeaveRequestTemplateStore
{
    LegacyLeaveRequestTemplate? Find(string code);
    void Save(LegacyLeaveRequestTemplate item);
    bool Remove(string code);
    List<LegacyLeaveRequestTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyLeaveRequestTemplateStore(Dictionary<string, LegacyLeaveRequestTemplate> items, List<string> order) : ILegacyLeaveRequestTemplateStore
{
    public Dictionary<string, LegacyLeaveRequestTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyLeaveRequestTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyLeaveRequestTemplate item)
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

    public List<LegacyLeaveRequestTemplate> List()
    {
        var outValue = new List<LegacyLeaveRequestTemplate>();
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
public sealed class CachedLegacyLeaveRequestTemplateStore(MemoryLegacyLeaveRequestTemplateStore inner, LegacyLeaveRequestTemplateCache cache, int hits, int misses) : ILegacyLeaveRequestTemplateStore
{
    public MemoryLegacyLeaveRequestTemplateStore Inner { get; set; } = inner;
    public LegacyLeaveRequestTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyLeaveRequestTemplate? Find(string code)
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

    public void Save(LegacyLeaveRequestTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyLeaveRequestTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy leave request templates. Empty fields match all.</summary>
public sealed record LegacyLeaveRequestTemplateQuery(string Label, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyLeaveRequestTemplate item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyLeaveRequestTemplate> Apply(List<LegacyLeaveRequestTemplate> items)
    {
        var matched = Collections.Filter(items, (LegacyLeaveRequestTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy leave request template store.</summary>
public static class LegacyLeaveRequestTemplateStore
{
    public static ILegacyLeaveRequestTemplateStore NewMemoryLegacyLeaveRequestTemplateStore()
    {
        return new MemoryLegacyLeaveRequestTemplateStore(
            new Dictionary<string, LegacyLeaveRequestTemplate>(),
            new List<string>());
    }

    public static ILegacyLeaveRequestTemplateStore NewCachedLegacyLeaveRequestTemplateStore(MemoryLegacyLeaveRequestTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyLeaveRequestTemplateStore(
            inner,
            LegacyLeaveRequestTemplateCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LegacyLeaveRequestTemplate> FindLegacyLeaveRequestTemplates(ILegacyLeaveRequestTemplateStore store, LegacyLeaveRequestTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
