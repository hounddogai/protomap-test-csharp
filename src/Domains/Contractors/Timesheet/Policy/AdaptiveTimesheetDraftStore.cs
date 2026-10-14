namespace Acme.Workforce.Contractors.Timesheet.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive timesheet drafts by code.</summary>
public interface IAdaptiveTimesheetDraftStore
{
    AdaptiveTimesheetDraft? Find(string code);
    void Save(AdaptiveTimesheetDraft item);
    bool Remove(string code);
    List<AdaptiveTimesheetDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveTimesheetDraftStore(Dictionary<string, AdaptiveTimesheetDraft> items, List<string> order) : IAdaptiveTimesheetDraftStore
{
    public Dictionary<string, AdaptiveTimesheetDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveTimesheetDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveTimesheetDraft item)
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

    public List<AdaptiveTimesheetDraft> List()
    {
        var outValue = new List<AdaptiveTimesheetDraft>();
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
public sealed class CachedAdaptiveTimesheetDraftStore(MemoryAdaptiveTimesheetDraftStore inner, AdaptiveTimesheetDraftCache cache, int hits, int misses) : IAdaptiveTimesheetDraftStore
{
    public MemoryAdaptiveTimesheetDraftStore Inner { get; set; } = inner;
    public AdaptiveTimesheetDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveTimesheetDraft? Find(string code)
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

    public void Save(AdaptiveTimesheetDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveTimesheetDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive timesheet drafts. Empty fields match all.</summary>
public sealed record AdaptiveTimesheetDraftQuery(string Name, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveTimesheetDraft item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<AdaptiveTimesheetDraft> Apply(List<AdaptiveTimesheetDraft> items)
    {
        var matched = Collections.Filter(items, (AdaptiveTimesheetDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive timesheet draft store.</summary>
public static class AdaptiveTimesheetDraftStore
{
    public static IAdaptiveTimesheetDraftStore NewMemoryAdaptiveTimesheetDraftStore()
    {
        return new MemoryAdaptiveTimesheetDraftStore(
            new Dictionary<string, AdaptiveTimesheetDraft>(),
            new List<string>());
    }

    public static IAdaptiveTimesheetDraftStore NewCachedAdaptiveTimesheetDraftStore(MemoryAdaptiveTimesheetDraftStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveTimesheetDraftStore(
            inner,
            AdaptiveTimesheetDraftCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveTimesheetDraft> FindAdaptiveTimesheetDrafts(IAdaptiveTimesheetDraftStore store, AdaptiveTimesheetDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
