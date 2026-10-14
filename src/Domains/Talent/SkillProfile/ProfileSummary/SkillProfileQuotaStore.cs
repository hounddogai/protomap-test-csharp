namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile quotas by code.</summary>
public interface ISkillProfileQuotaStore
{
    SkillProfileQuota? Find(string code);
    void Save(SkillProfileQuota item);
    bool Remove(string code);
    List<SkillProfileQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfileQuotaStore(Dictionary<string, SkillProfileQuota> items, List<string> order) : ISkillProfileQuotaStore
{
    public Dictionary<string, SkillProfileQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfileQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfileQuota item)
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

    public List<SkillProfileQuota> List()
    {
        var outValue = new List<SkillProfileQuota>();
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
public sealed class CachedSkillProfileQuotaStore(MemorySkillProfileQuotaStore inner, SkillProfileQuotaCache cache, int hits, int misses) : ISkillProfileQuotaStore
{
    public MemorySkillProfileQuotaStore Inner { get; set; } = inner;
    public SkillProfileQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfileQuota? Find(string code)
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

    public void Save(SkillProfileQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfileQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile quotas. Empty fields match all.</summary>
public sealed record SkillProfileQuotaQuery(string Label, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfileQuota item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SkillProfileQuota> Apply(List<SkillProfileQuota> items)
    {
        var matched = Collections.Filter(items, (SkillProfileQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile quota store.</summary>
public static class SkillProfileQuotaStore
{
    public static ISkillProfileQuotaStore NewMemorySkillProfileQuotaStore()
    {
        return new MemorySkillProfileQuotaStore(new Dictionary<string, SkillProfileQuota>(), new List<string>());
    }

    public static ISkillProfileQuotaStore NewCachedSkillProfileQuotaStore(MemorySkillProfileQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfileQuotaStore(inner, SkillProfileQuotaCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<SkillProfileQuota> FindSkillProfileQuotas(ISkillProfileQuotaStore store, SkillProfileQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
