namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual drafts by code.</summary>
public interface IAccrualDraftStore
{
    AccrualDraft? Find(string code);
    void Save(AccrualDraft item);
    bool Remove(string code);
    List<AccrualDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualDraftStore(Dictionary<string, AccrualDraft> items, List<string> order) : IAccrualDraftStore
{
    public Dictionary<string, AccrualDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualDraft item)
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

    public List<AccrualDraft> List()
    {
        var outValue = new List<AccrualDraft>();
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
public sealed class CachedAccrualDraftStore(MemoryAccrualDraftStore inner, AccrualDraftCache cache, int hits, int misses) : IAccrualDraftStore
{
    public MemoryAccrualDraftStore Inner { get; set; } = inner;
    public AccrualDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualDraft? Find(string code)
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

    public void Save(AccrualDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual drafts. Empty fields match all.</summary>
public sealed record AccrualDraftQuery(string Country, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualDraft item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccrualDraft> Apply(List<AccrualDraft> items)
    {
        var matched = Collections.Filter(items, (AccrualDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual draft store.</summary>
public static class AccrualDraftStore
{
    public static IAccrualDraftStore NewMemoryAccrualDraftStore()
    {
        return new MemoryAccrualDraftStore(new Dictionary<string, AccrualDraft>(), new List<string>());
    }

    public static IAccrualDraftStore NewCachedAccrualDraftStore(MemoryAccrualDraftStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualDraftStore(inner, AccrualDraftCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AccrualDraft> FindAccrualDrafts(IAccrualDraftStore store, AccrualDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
