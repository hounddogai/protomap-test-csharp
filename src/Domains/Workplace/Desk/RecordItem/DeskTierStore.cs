namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk tiers by code.</summary>
public interface IDeskTierStore
{
    DeskTier? Find(string code);
    void Save(DeskTier item);
    bool Remove(string code);
    List<DeskTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskTierStore(Dictionary<string, DeskTier> items, List<string> order) : IDeskTierStore
{
    public Dictionary<string, DeskTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskTier item)
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

    public List<DeskTier> List()
    {
        var outValue = new List<DeskTier>();
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
public sealed class CachedDeskTierStore(MemoryDeskTierStore inner, DeskTierCache cache, int hits, int misses) : IDeskTierStore
{
    public MemoryDeskTierStore Inner { get; set; } = inner;
    public DeskTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskTier? Find(string code)
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

    public void Save(DeskTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk tiers. Empty fields match all.</summary>
public sealed record DeskTierQuery(string Team, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(DeskTier item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskTier> Apply(List<DeskTier> items)
    {
        var matched = Collections.Filter(items, (DeskTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk tier store.</summary>
public static class DeskTierStore
{
    public static IDeskTierStore NewMemoryDeskTierStore()
    {
        return new MemoryDeskTierStore(new Dictionary<string, DeskTier>(), new List<string>());
    }

    public static IDeskTierStore NewCachedDeskTierStore(MemoryDeskTierStore inner, int ttl, IClock clock)
    {
        return new CachedDeskTierStore(inner, DeskTierCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DeskTier> FindDeskTiers(IDeskTierStore store, DeskTierQuery query)
    {
        return query.Apply(store.List());
    }
}
