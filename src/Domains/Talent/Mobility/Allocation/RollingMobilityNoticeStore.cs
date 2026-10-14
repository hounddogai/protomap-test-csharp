namespace Acme.Workforce.Talent.Mobility.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling mobility notices by code.</summary>
public interface IRollingMobilityNoticeStore
{
    RollingMobilityNotice? Find(string code);
    void Save(RollingMobilityNotice item);
    bool Remove(string code);
    List<RollingMobilityNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingMobilityNoticeStore(Dictionary<string, RollingMobilityNotice> items, List<string> order) : IRollingMobilityNoticeStore
{
    public Dictionary<string, RollingMobilityNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingMobilityNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingMobilityNotice item)
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

    public List<RollingMobilityNotice> List()
    {
        var outValue = new List<RollingMobilityNotice>();
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
public sealed class CachedRollingMobilityNoticeStore(MemoryRollingMobilityNoticeStore inner, RollingMobilityNoticeCache cache, int hits, int misses) : IRollingMobilityNoticeStore
{
    public MemoryRollingMobilityNoticeStore Inner { get; set; } = inner;
    public RollingMobilityNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingMobilityNotice? Find(string code)
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

    public void Save(RollingMobilityNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingMobilityNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling mobility notices. Empty fields match all.</summary>
public sealed record RollingMobilityNoticeQuery(string Name, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(RollingMobilityNotice item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingMobilityNotice> Apply(List<RollingMobilityNotice> items)
    {
        var matched = Collections.Filter(items, (RollingMobilityNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling mobility notice store.</summary>
public static class RollingMobilityNoticeStore
{
    public static IRollingMobilityNoticeStore NewMemoryRollingMobilityNoticeStore()
    {
        return new MemoryRollingMobilityNoticeStore(
            new Dictionary<string, RollingMobilityNotice>(),
            new List<string>());
    }

    public static IRollingMobilityNoticeStore NewCachedRollingMobilityNoticeStore(MemoryRollingMobilityNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedRollingMobilityNoticeStore(inner, RollingMobilityNoticeCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RollingMobilityNotice> FindRollingMobilityNotices(IRollingMobilityNoticeStore store, RollingMobilityNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
