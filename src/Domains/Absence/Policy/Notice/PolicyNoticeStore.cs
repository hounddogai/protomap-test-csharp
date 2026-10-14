namespace Acme.Workforce.Absence.Policy.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy notices by code.</summary>
public interface IPolicyNoticeStore
{
    PolicyNotice? Find(string code);
    void Save(PolicyNotice item);
    bool Remove(string code);
    List<PolicyNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyNoticeStore(Dictionary<string, PolicyNotice> items, List<string> order) : IPolicyNoticeStore
{
    public Dictionary<string, PolicyNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyNotice item)
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

    public List<PolicyNotice> List()
    {
        var outValue = new List<PolicyNotice>();
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
public sealed class CachedPolicyNoticeStore(MemoryPolicyNoticeStore inner, PolicyNoticeCache cache, int hits, int misses) : IPolicyNoticeStore
{
    public MemoryPolicyNoticeStore Inner { get; set; } = inner;
    public PolicyNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyNotice? Find(string code)
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

    public void Save(PolicyNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy notices. Empty fields match all.</summary>
public sealed record PolicyNoticeQuery(string Segment, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyNotice item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyNotice> Apply(List<PolicyNotice> items)
    {
        var matched = Collections.Filter(items, (PolicyNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy notice store.</summary>
public static class PolicyNoticeStore
{
    public static IPolicyNoticeStore NewMemoryPolicyNoticeStore()
    {
        return new MemoryPolicyNoticeStore(new Dictionary<string, PolicyNotice>(), new List<string>());
    }

    public static IPolicyNoticeStore NewCachedPolicyNoticeStore(MemoryPolicyNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyNoticeStore(inner, PolicyNoticeCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PolicyNotice> FindPolicyNotices(IPolicyNoticeStore store, PolicyNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
