namespace Acme.Workforce.Engagement.ActionPlan.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient action plan exceptions by code.</summary>
public interface ILenientActionPlanExceptionStore
{
    LenientActionPlanException? Find(string code);
    void Save(LenientActionPlanException item);
    bool Remove(string code);
    List<LenientActionPlanException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientActionPlanExceptionStore(Dictionary<string, LenientActionPlanException> items, List<string> order) : ILenientActionPlanExceptionStore
{
    public Dictionary<string, LenientActionPlanException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientActionPlanException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientActionPlanException item)
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

    public List<LenientActionPlanException> List()
    {
        var outValue = new List<LenientActionPlanException>();
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
public sealed class CachedLenientActionPlanExceptionStore(MemoryLenientActionPlanExceptionStore inner, LenientActionPlanExceptionCache cache, int hits, int misses) : ILenientActionPlanExceptionStore
{
    public MemoryLenientActionPlanExceptionStore Inner { get; set; } = inner;
    public LenientActionPlanExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientActionPlanException? Find(string code)
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

    public void Save(LenientActionPlanException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientActionPlanException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient action plan exceptions. Empty fields match all.</summary>
public sealed record LenientActionPlanExceptionQuery(string Label, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(LenientActionPlanException item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientActionPlanException> Apply(List<LenientActionPlanException> items)
    {
        var matched = Collections.Filter(items, (LenientActionPlanException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient action plan exception store.</summary>
public static class LenientActionPlanExceptionStore
{
    public static ILenientActionPlanExceptionStore NewMemoryLenientActionPlanExceptionStore()
    {
        return new MemoryLenientActionPlanExceptionStore(
            new Dictionary<string, LenientActionPlanException>(),
            new List<string>());
    }

    public static ILenientActionPlanExceptionStore NewCachedLenientActionPlanExceptionStore(MemoryLenientActionPlanExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedLenientActionPlanExceptionStore(
            inner,
            LenientActionPlanExceptionCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LenientActionPlanException> FindLenientActionPlanExceptions(ILenientActionPlanExceptionStore store, LenientActionPlanExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
