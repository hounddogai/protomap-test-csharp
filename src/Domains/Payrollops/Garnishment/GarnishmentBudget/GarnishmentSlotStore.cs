namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment slots by code.</summary>
public interface IGarnishmentSlotStore
{
    GarnishmentSlot? Find(string code);
    void Save(GarnishmentSlot item);
    bool Remove(string code);
    List<GarnishmentSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentSlotStore(Dictionary<string, GarnishmentSlot> items, List<string> order) : IGarnishmentSlotStore
{
    public Dictionary<string, GarnishmentSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentSlot item)
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

    public List<GarnishmentSlot> List()
    {
        var outValue = new List<GarnishmentSlot>();
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
public sealed class CachedGarnishmentSlotStore(MemoryGarnishmentSlotStore inner, GarnishmentSlotCache cache, int hits, int misses) : IGarnishmentSlotStore
{
    public MemoryGarnishmentSlotStore Inner { get; set; } = inner;
    public GarnishmentSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentSlot? Find(string code)
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

    public void Save(GarnishmentSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment slots. Empty fields match all.</summary>
public sealed record GarnishmentSlotQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentSlot item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GarnishmentSlot> Apply(List<GarnishmentSlot> items)
    {
        var matched = Collections.Filter(items, (GarnishmentSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment slot store.</summary>
public static class GarnishmentSlotStore
{
    public static IGarnishmentSlotStore NewMemoryGarnishmentSlotStore()
    {
        return new MemoryGarnishmentSlotStore(new Dictionary<string, GarnishmentSlot>(), new List<string>());
    }

    public static IGarnishmentSlotStore NewCachedGarnishmentSlotStore(MemoryGarnishmentSlotStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentSlotStore(inner, GarnishmentSlotCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<GarnishmentSlot> FindGarnishmentSlots(IGarnishmentSlotStore store, GarnishmentSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
