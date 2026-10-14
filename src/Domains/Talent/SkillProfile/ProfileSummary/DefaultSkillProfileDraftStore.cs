namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default skill profile drafts by code.</summary>
public interface IDefaultSkillProfileDraftStore
{
    DefaultSkillProfileDraft? Find(string code);
    void Save(DefaultSkillProfileDraft item);
    bool Remove(string code);
    List<DefaultSkillProfileDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultSkillProfileDraftStore(Dictionary<string, DefaultSkillProfileDraft> items, List<string> order) : IDefaultSkillProfileDraftStore
{
    public Dictionary<string, DefaultSkillProfileDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultSkillProfileDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultSkillProfileDraft item)
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

    public List<DefaultSkillProfileDraft> List()
    {
        var outValue = new List<DefaultSkillProfileDraft>();
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
public sealed class CachedDefaultSkillProfileDraftStore(MemoryDefaultSkillProfileDraftStore inner, DefaultSkillProfileDraftCache cache, int hits, int misses) : IDefaultSkillProfileDraftStore
{
    public MemoryDefaultSkillProfileDraftStore Inner { get; set; } = inner;
    public DefaultSkillProfileDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultSkillProfileDraft? Find(string code)
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

    public void Save(DefaultSkillProfileDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultSkillProfileDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default skill profile drafts. Empty fields match all.</summary>
public sealed record DefaultSkillProfileDraftQuery(string Department, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultSkillProfileDraft item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultSkillProfileDraft> Apply(List<DefaultSkillProfileDraft> items)
    {
        var matched = Collections.Filter(items, (DefaultSkillProfileDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default skill profile draft store.</summary>
public static class DefaultSkillProfileDraftStore
{
    public static IDefaultSkillProfileDraftStore NewMemoryDefaultSkillProfileDraftStore()
    {
        return new MemoryDefaultSkillProfileDraftStore(
            new Dictionary<string, DefaultSkillProfileDraft>(),
            new List<string>());
    }

    public static IDefaultSkillProfileDraftStore NewCachedDefaultSkillProfileDraftStore(MemoryDefaultSkillProfileDraftStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultSkillProfileDraftStore(
            inner,
            DefaultSkillProfileDraftCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<DefaultSkillProfileDraft> FindDefaultSkillProfileDrafts(IDefaultSkillProfileDraftStore store, DefaultSkillProfileDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
