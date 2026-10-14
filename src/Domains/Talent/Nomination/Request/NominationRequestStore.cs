namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination requests by code.</summary>
public interface INominationRequestStore
{
    NominationRequest? Find(string code);
    void Save(NominationRequest item);
    bool Remove(string code);
    List<NominationRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationRequestStore(Dictionary<string, NominationRequest> items, List<string> order) : INominationRequestStore
{
    public Dictionary<string, NominationRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationRequest item)
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

    public List<NominationRequest> List()
    {
        var outValue = new List<NominationRequest>();
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
public sealed class CachedNominationRequestStore(MemoryNominationRequestStore inner, NominationRequestCache cache, int hits, int misses) : INominationRequestStore
{
    public MemoryNominationRequestStore Inner { get; set; } = inner;
    public NominationRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationRequest? Find(string code)
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

    public void Save(NominationRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination requests. Empty fields match all.</summary>
public sealed record NominationRequestQuery(string Title, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(NominationRequest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<NominationRequest> Apply(List<NominationRequest> items)
    {
        var matched = Collections.Filter(items, (NominationRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination request store.</summary>
public static class NominationRequestStore
{
    public static INominationRequestStore NewMemoryNominationRequestStore()
    {
        return new MemoryNominationRequestStore(new Dictionary<string, NominationRequest>(), new List<string>());
    }

    public static INominationRequestStore NewCachedNominationRequestStore(MemoryNominationRequestStore inner, int ttl, IClock clock)
    {
        return new CachedNominationRequestStore(inner, NominationRequestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<NominationRequest> FindNominationRequests(INominationRequestStore store, NominationRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
