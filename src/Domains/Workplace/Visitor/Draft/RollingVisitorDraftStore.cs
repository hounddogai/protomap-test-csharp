namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling visitor drafts by code.</summary>
public interface IRollingVisitorDraftStore
{
    RollingVisitorDraft? Find(string code);
    void Save(RollingVisitorDraft item);
    bool Remove(string code);
    List<RollingVisitorDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingVisitorDraftStore(Dictionary<string, RollingVisitorDraft> items, List<string> order) : IRollingVisitorDraftStore
{
    public Dictionary<string, RollingVisitorDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingVisitorDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingVisitorDraft item)
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

    public List<RollingVisitorDraft> List()
    {
        var outValue = new List<RollingVisitorDraft>();
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
public sealed class CachedRollingVisitorDraftStore(MemoryRollingVisitorDraftStore inner, RollingVisitorDraftCache cache, int hits, int misses) : IRollingVisitorDraftStore
{
    public MemoryRollingVisitorDraftStore Inner { get; set; } = inner;
    public RollingVisitorDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingVisitorDraft? Find(string code)
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

    public void Save(RollingVisitorDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingVisitorDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling visitor drafts. Empty fields match all.</summary>
public sealed record RollingVisitorDraftQuery(string Team, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(RollingVisitorDraft item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<RollingVisitorDraft> Apply(List<RollingVisitorDraft> items)
    {
        var matched = Collections.Filter(items, (RollingVisitorDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling visitor draft store.</summary>
public static class RollingVisitorDraftStore
{
    public static IRollingVisitorDraftStore NewMemoryRollingVisitorDraftStore()
    {
        return new MemoryRollingVisitorDraftStore(new Dictionary<string, RollingVisitorDraft>(), new List<string>());
    }

    public static IRollingVisitorDraftStore NewCachedRollingVisitorDraftStore(MemoryRollingVisitorDraftStore inner, int ttl, IClock clock)
    {
        return new CachedRollingVisitorDraftStore(inner, RollingVisitorDraftCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RollingVisitorDraft> FindRollingVisitorDrafts(IRollingVisitorDraftStore store, RollingVisitorDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
