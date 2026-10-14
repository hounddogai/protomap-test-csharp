namespace Acme.Workforce.Workplace.Occupancy.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy drafts by code.</summary>
public interface IOccupancyDraftStore
{
    OccupancyDraft? Find(string code);
    void Save(OccupancyDraft item);
    bool Remove(string code);
    List<OccupancyDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyDraftStore(Dictionary<string, OccupancyDraft> items, List<string> order) : IOccupancyDraftStore
{
    public Dictionary<string, OccupancyDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyDraft item)
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

    public List<OccupancyDraft> List()
    {
        var outValue = new List<OccupancyDraft>();
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
public sealed class CachedOccupancyDraftStore(MemoryOccupancyDraftStore inner, OccupancyDraftCache cache, int hits, int misses) : IOccupancyDraftStore
{
    public MemoryOccupancyDraftStore Inner { get; set; } = inner;
    public OccupancyDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyDraft? Find(string code)
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

    public void Save(OccupancyDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy drafts. Empty fields match all.</summary>
public sealed record OccupancyDraftQuery(string Source, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyDraft item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OccupancyDraft> Apply(List<OccupancyDraft> items)
    {
        var matched = Collections.Filter(items, (OccupancyDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy draft store.</summary>
public static class OccupancyDraftStore
{
    public static IOccupancyDraftStore NewMemoryOccupancyDraftStore()
    {
        return new MemoryOccupancyDraftStore(new Dictionary<string, OccupancyDraft>(), new List<string>());
    }

    public static IOccupancyDraftStore NewCachedOccupancyDraftStore(MemoryOccupancyDraftStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyDraftStore(inner, OccupancyDraftCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<OccupancyDraft> FindOccupancyDrafts(IOccupancyDraftStore store, OccupancyDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
