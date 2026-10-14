namespace Acme.Workforce.Payrollops.Deduction.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction drafts by code.</summary>
public interface IDeductionDraftStore
{
    DeductionDraft? Find(string code);
    void Save(DeductionDraft item);
    bool Remove(string code);
    List<DeductionDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionDraftStore(Dictionary<string, DeductionDraft> items, List<string> order) : IDeductionDraftStore
{
    public Dictionary<string, DeductionDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionDraft item)
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

    public List<DeductionDraft> List()
    {
        var outValue = new List<DeductionDraft>();
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
public sealed class CachedDeductionDraftStore(MemoryDeductionDraftStore inner, DeductionDraftCache cache, int hits, int misses) : IDeductionDraftStore
{
    public MemoryDeductionDraftStore Inner { get; set; } = inner;
    public DeductionDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionDraft? Find(string code)
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

    public void Save(DeductionDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction drafts. Empty fields match all.</summary>
public sealed record DeductionDraftQuery(string Department, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionDraft item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeductionDraft> Apply(List<DeductionDraft> items)
    {
        var matched = Collections.Filter(items, (DeductionDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction draft store.</summary>
public static class DeductionDraftStore
{
    public static IDeductionDraftStore NewMemoryDeductionDraftStore()
    {
        return new MemoryDeductionDraftStore(new Dictionary<string, DeductionDraft>(), new List<string>());
    }

    public static IDeductionDraftStore NewCachedDeductionDraftStore(MemoryDeductionDraftStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionDraftStore(inner, DeductionDraftCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DeductionDraft> FindDeductionDrafts(IDeductionDraftStore store, DeductionDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
