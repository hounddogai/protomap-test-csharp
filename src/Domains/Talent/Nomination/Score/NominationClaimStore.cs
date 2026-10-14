namespace Acme.Workforce.Talent.Nomination.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination claims by code.</summary>
public interface INominationClaimStore
{
    NominationClaim? Find(string code);
    void Save(NominationClaim item);
    bool Remove(string code);
    List<NominationClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationClaimStore(Dictionary<string, NominationClaim> items, List<string> order) : INominationClaimStore
{
    public Dictionary<string, NominationClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationClaim item)
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

    public List<NominationClaim> List()
    {
        var outValue = new List<NominationClaim>();
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
public sealed class CachedNominationClaimStore(MemoryNominationClaimStore inner, NominationClaimCache cache, int hits, int misses) : INominationClaimStore
{
    public MemoryNominationClaimStore Inner { get; set; } = inner;
    public NominationClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationClaim? Find(string code)
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

    public void Save(NominationClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination claims. Empty fields match all.</summary>
public sealed record NominationClaimQuery(string Title, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(NominationClaim item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<NominationClaim> Apply(List<NominationClaim> items)
    {
        var matched = Collections.Filter(items, (NominationClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination claim store.</summary>
public static class NominationClaimStore
{
    public static INominationClaimStore NewMemoryNominationClaimStore()
    {
        return new MemoryNominationClaimStore(new Dictionary<string, NominationClaim>(), new List<string>());
    }

    public static INominationClaimStore NewCachedNominationClaimStore(MemoryNominationClaimStore inner, int ttl, IClock clock)
    {
        return new CachedNominationClaimStore(inner, NominationClaimCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<NominationClaim> FindNominationClaims(INominationClaimStore store, NominationClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
