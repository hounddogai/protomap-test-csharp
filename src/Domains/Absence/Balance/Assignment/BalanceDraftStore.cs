namespace Acme.Workforce.Absence.Balance.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance drafts by code.</summary>
public interface IBalanceDraftStore
{
    BalanceDraft? Find(string code);
    void Save(BalanceDraft item);
    bool Remove(string code);
    List<BalanceDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalanceDraftStore(Dictionary<string, BalanceDraft> items, List<string> order) : IBalanceDraftStore
{
    public Dictionary<string, BalanceDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalanceDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalanceDraft item)
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

    public List<BalanceDraft> List()
    {
        var outValue = new List<BalanceDraft>();
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
public sealed class CachedBalanceDraftStore(MemoryBalanceDraftStore inner, BalanceDraftCache cache, int hits, int misses) : IBalanceDraftStore
{
    public MemoryBalanceDraftStore Inner { get; set; } = inner;
    public BalanceDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalanceDraft? Find(string code)
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

    public void Save(BalanceDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalanceDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance drafts. Empty fields match all.</summary>
public sealed record BalanceDraftQuery(string Channel, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(BalanceDraft item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<BalanceDraft> Apply(List<BalanceDraft> items)
    {
        var matched = Collections.Filter(items, (BalanceDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance draft store.</summary>
public static class BalanceDraftStore
{
    public static IBalanceDraftStore NewMemoryBalanceDraftStore()
    {
        return new MemoryBalanceDraftStore(new Dictionary<string, BalanceDraft>(), new List<string>());
    }

    public static IBalanceDraftStore NewCachedBalanceDraftStore(MemoryBalanceDraftStore inner, int ttl, IClock clock)
    {
        return new CachedBalanceDraftStore(inner, BalanceDraftCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BalanceDraft> FindBalanceDrafts(IBalanceDraftStore store, BalanceDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
