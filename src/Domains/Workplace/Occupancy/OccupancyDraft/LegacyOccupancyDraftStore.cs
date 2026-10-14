namespace Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy occupancy drafts by code.</summary>
public interface ILegacyOccupancyDraftStore
{
    LegacyOccupancyDraft? Find(string code);
    void Save(LegacyOccupancyDraft item);
    bool Remove(string code);
    List<LegacyOccupancyDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyOccupancyDraftStore(Dictionary<string, LegacyOccupancyDraft> items, List<string> order) : ILegacyOccupancyDraftStore
{
    public Dictionary<string, LegacyOccupancyDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyOccupancyDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyOccupancyDraft item)
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

    public List<LegacyOccupancyDraft> List()
    {
        var outValue = new List<LegacyOccupancyDraft>();
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
public sealed class CachedLegacyOccupancyDraftStore(MemoryLegacyOccupancyDraftStore inner, LegacyOccupancyDraftCache cache, int hits, int misses) : ILegacyOccupancyDraftStore
{
    public MemoryLegacyOccupancyDraftStore Inner { get; set; } = inner;
    public LegacyOccupancyDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyOccupancyDraft? Find(string code)
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

    public void Save(LegacyOccupancyDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyOccupancyDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy occupancy drafts. Empty fields match all.</summary>
public sealed record LegacyOccupancyDraftQuery(string Department, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyOccupancyDraft item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<LegacyOccupancyDraft> Apply(List<LegacyOccupancyDraft> items)
    {
        var matched = Collections.Filter(items, (LegacyOccupancyDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy occupancy draft store.</summary>
public static class LegacyOccupancyDraftStore
{
    public static ILegacyOccupancyDraftStore NewMemoryLegacyOccupancyDraftStore()
    {
        return new MemoryLegacyOccupancyDraftStore(
            new Dictionary<string, LegacyOccupancyDraft>(),
            new List<string>());
    }

    public static ILegacyOccupancyDraftStore NewCachedLegacyOccupancyDraftStore(MemoryLegacyOccupancyDraftStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyOccupancyDraftStore(inner, LegacyOccupancyDraftCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LegacyOccupancyDraft> FindLegacyOccupancyDrafts(ILegacyOccupancyDraftStore store, LegacyOccupancyDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
