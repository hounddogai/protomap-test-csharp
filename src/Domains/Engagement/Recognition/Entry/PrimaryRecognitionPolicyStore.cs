namespace Acme.Workforce.Engagement.Recognition.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary recognition policies by code.</summary>
public interface IPrimaryRecognitionPolicyStore
{
    PrimaryRecognitionPolicy? Find(string code);
    void Save(PrimaryRecognitionPolicy item);
    bool Remove(string code);
    List<PrimaryRecognitionPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryRecognitionPolicyStore(Dictionary<string, PrimaryRecognitionPolicy> items, List<string> order) : IPrimaryRecognitionPolicyStore
{
    public Dictionary<string, PrimaryRecognitionPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryRecognitionPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryRecognitionPolicy item)
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

    public List<PrimaryRecognitionPolicy> List()
    {
        var outValue = new List<PrimaryRecognitionPolicy>();
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
public sealed class CachedPrimaryRecognitionPolicyStore(MemoryPrimaryRecognitionPolicyStore inner, PrimaryRecognitionPolicyCache cache, int hits, int misses) : IPrimaryRecognitionPolicyStore
{
    public MemoryPrimaryRecognitionPolicyStore Inner { get; set; } = inner;
    public PrimaryRecognitionPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryRecognitionPolicy? Find(string code)
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

    public void Save(PrimaryRecognitionPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryRecognitionPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary recognition policies. Empty fields match all.</summary>
public sealed record PrimaryRecognitionPolicyQuery(string Source, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryRecognitionPolicy item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryRecognitionPolicy> Apply(List<PrimaryRecognitionPolicy> items)
    {
        var matched = Collections.Filter(items, (PrimaryRecognitionPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary recognition policy store.</summary>
public static class PrimaryRecognitionPolicyStore
{
    public static IPrimaryRecognitionPolicyStore NewMemoryPrimaryRecognitionPolicyStore()
    {
        return new MemoryPrimaryRecognitionPolicyStore(
            new Dictionary<string, PrimaryRecognitionPolicy>(),
            new List<string>());
    }

    public static IPrimaryRecognitionPolicyStore NewCachedPrimaryRecognitionPolicyStore(MemoryPrimaryRecognitionPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryRecognitionPolicyStore(
            inner,
            PrimaryRecognitionPolicyCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryRecognitionPolicy> FindPrimaryRecognitionPolicies(IPrimaryRecognitionPolicyStore store, PrimaryRecognitionPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
