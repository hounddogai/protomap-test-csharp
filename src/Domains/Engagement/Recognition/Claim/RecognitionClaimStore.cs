namespace Acme.Workforce.Engagement.Recognition.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition claims by code.</summary>
public interface IRecognitionClaimStore
{
    RecognitionClaim? Find(string code);
    void Save(RecognitionClaim item);
    bool Remove(string code);
    List<RecognitionClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionClaimStore(Dictionary<string, RecognitionClaim> items, List<string> order) : IRecognitionClaimStore
{
    public Dictionary<string, RecognitionClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionClaim item)
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

    public List<RecognitionClaim> List()
    {
        var outValue = new List<RecognitionClaim>();
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
public sealed class CachedRecognitionClaimStore(MemoryRecognitionClaimStore inner, RecognitionClaimCache cache, int hits, int misses) : IRecognitionClaimStore
{
    public MemoryRecognitionClaimStore Inner { get; set; } = inner;
    public RecognitionClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionClaim? Find(string code)
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

    public void Save(RecognitionClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition claims. Empty fields match all.</summary>
public sealed record RecognitionClaimQuery(string Channel, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionClaim item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RecognitionClaim> Apply(List<RecognitionClaim> items)
    {
        var matched = Collections.Filter(items, (RecognitionClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition claim store.</summary>
public static class RecognitionClaimStore
{
    public static IRecognitionClaimStore NewMemoryRecognitionClaimStore()
    {
        return new MemoryRecognitionClaimStore(new Dictionary<string, RecognitionClaim>(), new List<string>());
    }

    public static IRecognitionClaimStore NewCachedRecognitionClaimStore(MemoryRecognitionClaimStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionClaimStore(inner, RecognitionClaimCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RecognitionClaim> FindRecognitionClaims(IRecognitionClaimStore store, RecognitionClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
