namespace Acme.Workforce.Engagement.Feedback.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary feedback claims by code.</summary>
public interface IPrimaryFeedbackClaimStore
{
    PrimaryFeedbackClaim? Find(string code);
    void Save(PrimaryFeedbackClaim item);
    bool Remove(string code);
    List<PrimaryFeedbackClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryFeedbackClaimStore(Dictionary<string, PrimaryFeedbackClaim> items, List<string> order) : IPrimaryFeedbackClaimStore
{
    public Dictionary<string, PrimaryFeedbackClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryFeedbackClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryFeedbackClaim item)
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

    public List<PrimaryFeedbackClaim> List()
    {
        var outValue = new List<PrimaryFeedbackClaim>();
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
public sealed class CachedPrimaryFeedbackClaimStore(MemoryPrimaryFeedbackClaimStore inner, PrimaryFeedbackClaimCache cache, int hits, int misses) : IPrimaryFeedbackClaimStore
{
    public MemoryPrimaryFeedbackClaimStore Inner { get; set; } = inner;
    public PrimaryFeedbackClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryFeedbackClaim? Find(string code)
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

    public void Save(PrimaryFeedbackClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryFeedbackClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary feedback claims. Empty fields match all.</summary>
public sealed record PrimaryFeedbackClaimQuery(string Segment, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryFeedbackClaim item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<PrimaryFeedbackClaim> Apply(List<PrimaryFeedbackClaim> items)
    {
        var matched = Collections.Filter(items, (PrimaryFeedbackClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary feedback claim store.</summary>
public static class PrimaryFeedbackClaimStore
{
    public static IPrimaryFeedbackClaimStore NewMemoryPrimaryFeedbackClaimStore()
    {
        return new MemoryPrimaryFeedbackClaimStore(
            new Dictionary<string, PrimaryFeedbackClaim>(),
            new List<string>());
    }

    public static IPrimaryFeedbackClaimStore NewCachedPrimaryFeedbackClaimStore(MemoryPrimaryFeedbackClaimStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryFeedbackClaimStore(inner, PrimaryFeedbackClaimCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PrimaryFeedbackClaim> FindPrimaryFeedbackClaims(IPrimaryFeedbackClaimStore store, PrimaryFeedbackClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
