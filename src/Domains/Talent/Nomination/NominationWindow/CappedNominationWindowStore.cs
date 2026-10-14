namespace Acme.Workforce.Talent.Nomination.NominationWindow;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped nomination windows by code.</summary>
public interface ICappedNominationWindowStore
{
    CappedNominationWindow? Find(string code);
    void Save(CappedNominationWindow item);
    bool Remove(string code);
    List<CappedNominationWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedNominationWindowStore(Dictionary<string, CappedNominationWindow> items, List<string> order) : ICappedNominationWindowStore
{
    public Dictionary<string, CappedNominationWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedNominationWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedNominationWindow item)
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

    public List<CappedNominationWindow> List()
    {
        var outValue = new List<CappedNominationWindow>();
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
public sealed class CachedCappedNominationWindowStore(MemoryCappedNominationWindowStore inner, CappedNominationWindowCache cache, int hits, int misses) : ICappedNominationWindowStore
{
    public MemoryCappedNominationWindowStore Inner { get; set; } = inner;
    public CappedNominationWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedNominationWindow? Find(string code)
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

    public void Save(CappedNominationWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedNominationWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped nomination windows. Empty fields match all.</summary>
public sealed record CappedNominationWindowQuery(string Label, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(CappedNominationWindow item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedNominationWindow> Apply(List<CappedNominationWindow> items)
    {
        var matched = Collections.Filter(items, (CappedNominationWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped nomination window store.</summary>
public static class CappedNominationWindowStore
{
    public static ICappedNominationWindowStore NewMemoryCappedNominationWindowStore()
    {
        return new MemoryCappedNominationWindowStore(
            new Dictionary<string, CappedNominationWindow>(),
            new List<string>());
    }

    public static ICappedNominationWindowStore NewCachedCappedNominationWindowStore(MemoryCappedNominationWindowStore inner, int ttl, IClock clock)
    {
        return new CachedCappedNominationWindowStore(
            inner,
            CappedNominationWindowCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<CappedNominationWindow> FindCappedNominationWindows(ICappedNominationWindowStore store, CappedNominationWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
