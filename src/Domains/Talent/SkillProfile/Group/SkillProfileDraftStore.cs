namespace Acme.Workforce.Talent.SkillProfile.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile drafts by code.</summary>
public interface ISkillProfileDraftStore
{
    SkillProfileDraft? Find(string code);
    void Save(SkillProfileDraft item);
    bool Remove(string code);
    List<SkillProfileDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfileDraftStore(Dictionary<string, SkillProfileDraft> items, List<string> order) : ISkillProfileDraftStore
{
    public Dictionary<string, SkillProfileDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfileDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfileDraft item)
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

    public List<SkillProfileDraft> List()
    {
        var outValue = new List<SkillProfileDraft>();
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
public sealed class CachedSkillProfileDraftStore(MemorySkillProfileDraftStore inner, SkillProfileDraftCache cache, int hits, int misses) : ISkillProfileDraftStore
{
    public MemorySkillProfileDraftStore Inner { get; set; } = inner;
    public SkillProfileDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfileDraft? Find(string code)
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

    public void Save(SkillProfileDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfileDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile drafts. Empty fields match all.</summary>
public sealed record SkillProfileDraftQuery(string Source, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfileDraft item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<SkillProfileDraft> Apply(List<SkillProfileDraft> items)
    {
        var matched = Collections.Filter(items, (SkillProfileDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile draft store.</summary>
public static class SkillProfileDraftStore
{
    public static ISkillProfileDraftStore NewMemorySkillProfileDraftStore()
    {
        return new MemorySkillProfileDraftStore(new Dictionary<string, SkillProfileDraft>(), new List<string>());
    }

    public static ISkillProfileDraftStore NewCachedSkillProfileDraftStore(MemorySkillProfileDraftStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfileDraftStore(inner, SkillProfileDraftCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<SkillProfileDraft> FindSkillProfileDrafts(ISkillProfileDraftStore store, SkillProfileDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
