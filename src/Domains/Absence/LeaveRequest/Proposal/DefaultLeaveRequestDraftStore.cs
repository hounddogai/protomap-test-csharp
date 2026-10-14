namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default leave request drafts by code.</summary>
public interface IDefaultLeaveRequestDraftStore
{
    DefaultLeaveRequestDraft? Find(string code);
    void Save(DefaultLeaveRequestDraft item);
    bool Remove(string code);
    List<DefaultLeaveRequestDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultLeaveRequestDraftStore(Dictionary<string, DefaultLeaveRequestDraft> items, List<string> order) : IDefaultLeaveRequestDraftStore
{
    public Dictionary<string, DefaultLeaveRequestDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultLeaveRequestDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultLeaveRequestDraft item)
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

    public List<DefaultLeaveRequestDraft> List()
    {
        var outValue = new List<DefaultLeaveRequestDraft>();
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
public sealed class CachedDefaultLeaveRequestDraftStore(MemoryDefaultLeaveRequestDraftStore inner, DefaultLeaveRequestDraftCache cache, int hits, int misses) : IDefaultLeaveRequestDraftStore
{
    public MemoryDefaultLeaveRequestDraftStore Inner { get; set; } = inner;
    public DefaultLeaveRequestDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultLeaveRequestDraft? Find(string code)
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

    public void Save(DefaultLeaveRequestDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultLeaveRequestDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default leave request drafts. Empty fields match all.</summary>
public sealed record DefaultLeaveRequestDraftQuery(string Segment, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultLeaveRequestDraft item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultLeaveRequestDraft> Apply(List<DefaultLeaveRequestDraft> items)
    {
        var matched = Collections.Filter(items, (DefaultLeaveRequestDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default leave request draft store.</summary>
public static class DefaultLeaveRequestDraftStore
{
    public static IDefaultLeaveRequestDraftStore NewMemoryDefaultLeaveRequestDraftStore()
    {
        return new MemoryDefaultLeaveRequestDraftStore(
            new Dictionary<string, DefaultLeaveRequestDraft>(),
            new List<string>());
    }

    public static IDefaultLeaveRequestDraftStore NewCachedDefaultLeaveRequestDraftStore(MemoryDefaultLeaveRequestDraftStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultLeaveRequestDraftStore(
            inner,
            DefaultLeaveRequestDraftCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<DefaultLeaveRequestDraft> FindDefaultLeaveRequestDrafts(IDefaultLeaveRequestDraftStore store, DefaultLeaveRequestDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
