namespace Acme.Workforce.Absence.Policy.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy profiles by code.</summary>
public interface IPolicyProfileStore
{
    PolicyProfile? Find(string code);
    void Save(PolicyProfile item);
    bool Remove(string code);
    List<PolicyProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyProfileStore(Dictionary<string, PolicyProfile> items, List<string> order) : IPolicyProfileStore
{
    public Dictionary<string, PolicyProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyProfile item)
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

    public List<PolicyProfile> List()
    {
        var outValue = new List<PolicyProfile>();
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
public sealed class CachedPolicyProfileStore(MemoryPolicyProfileStore inner, PolicyProfileCache cache, int hits, int misses) : IPolicyProfileStore
{
    public MemoryPolicyProfileStore Inner { get; set; } = inner;
    public PolicyProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyProfile? Find(string code)
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

    public void Save(PolicyProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy profiles. Empty fields match all.</summary>
public sealed record PolicyProfileQuery(string Category, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyProfile item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyProfile> Apply(List<PolicyProfile> items)
    {
        var matched = Collections.Filter(items, (PolicyProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy profile store.</summary>
public static class PolicyProfileStore
{
    public static IPolicyProfileStore NewMemoryPolicyProfileStore()
    {
        return new MemoryPolicyProfileStore(new Dictionary<string, PolicyProfile>(), new List<string>());
    }

    public static IPolicyProfileStore NewCachedPolicyProfileStore(MemoryPolicyProfileStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyProfileStore(inner, PolicyProfileCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PolicyProfile> FindPolicyProfiles(IPolicyProfileStore store, PolicyProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
