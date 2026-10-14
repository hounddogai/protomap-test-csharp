namespace Acme.Workforce.Talent.SkillProfile.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile notices by code.</summary>
public interface ISkillProfileNoticeStore
{
    SkillProfileNotice? Find(string code);
    void Save(SkillProfileNotice item);
    bool Remove(string code);
    List<SkillProfileNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfileNoticeStore(Dictionary<string, SkillProfileNotice> items, List<string> order) : ISkillProfileNoticeStore
{
    public Dictionary<string, SkillProfileNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfileNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfileNotice item)
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

    public List<SkillProfileNotice> List()
    {
        var outValue = new List<SkillProfileNotice>();
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
public sealed class CachedSkillProfileNoticeStore(MemorySkillProfileNoticeStore inner, SkillProfileNoticeCache cache, int hits, int misses) : ISkillProfileNoticeStore
{
    public MemorySkillProfileNoticeStore Inner { get; set; } = inner;
    public SkillProfileNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfileNotice? Find(string code)
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

    public void Save(SkillProfileNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfileNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile notices. Empty fields match all.</summary>
public sealed record SkillProfileNoticeQuery(string Team, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfileNotice item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SkillProfileNotice> Apply(List<SkillProfileNotice> items)
    {
        var matched = Collections.Filter(items, (SkillProfileNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile notice store.</summary>
public static class SkillProfileNoticeStore
{
    public static ISkillProfileNoticeStore NewMemorySkillProfileNoticeStore()
    {
        return new MemorySkillProfileNoticeStore(new Dictionary<string, SkillProfileNotice>(), new List<string>());
    }

    public static ISkillProfileNoticeStore NewCachedSkillProfileNoticeStore(MemorySkillProfileNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfileNoticeStore(inner, SkillProfileNoticeCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<SkillProfileNotice> FindSkillProfileNotices(ISkillProfileNoticeStore store, SkillProfileNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
