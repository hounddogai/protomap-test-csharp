namespace Acme.Workforce.Payrollops.Deduction.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient deduction drafts by code.</summary>
public interface ILenientDeductionDraftStore
{
    LenientDeductionDraft? Find(string code);
    void Save(LenientDeductionDraft item);
    bool Remove(string code);
    List<LenientDeductionDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientDeductionDraftStore(Dictionary<string, LenientDeductionDraft> items, List<string> order) : ILenientDeductionDraftStore
{
    public Dictionary<string, LenientDeductionDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientDeductionDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientDeductionDraft item)
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

    public List<LenientDeductionDraft> List()
    {
        var outValue = new List<LenientDeductionDraft>();
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
public sealed class CachedLenientDeductionDraftStore(MemoryLenientDeductionDraftStore inner, LenientDeductionDraftCache cache, int hits, int misses) : ILenientDeductionDraftStore
{
    public MemoryLenientDeductionDraftStore Inner { get; set; } = inner;
    public LenientDeductionDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientDeductionDraft? Find(string code)
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

    public void Save(LenientDeductionDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientDeductionDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient deduction drafts. Empty fields match all.</summary>
public sealed record LenientDeductionDraftQuery(string Category, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(LenientDeductionDraft item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientDeductionDraft> Apply(List<LenientDeductionDraft> items)
    {
        var matched = Collections.Filter(items, (LenientDeductionDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient deduction draft store.</summary>
public static class LenientDeductionDraftStore
{
    public static ILenientDeductionDraftStore NewMemoryLenientDeductionDraftStore()
    {
        return new MemoryLenientDeductionDraftStore(
            new Dictionary<string, LenientDeductionDraft>(),
            new List<string>());
    }

    public static ILenientDeductionDraftStore NewCachedLenientDeductionDraftStore(MemoryLenientDeductionDraftStore inner, int ttl, IClock clock)
    {
        return new CachedLenientDeductionDraftStore(inner, LenientDeductionDraftCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LenientDeductionDraft> FindLenientDeductionDrafts(ILenientDeductionDraftStore store, LenientDeductionDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
