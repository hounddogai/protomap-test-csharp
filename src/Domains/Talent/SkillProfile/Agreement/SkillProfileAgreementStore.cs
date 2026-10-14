namespace Acme.Workforce.Talent.SkillProfile.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile agreements by code.</summary>
public interface ISkillProfileAgreementStore
{
    SkillProfileAgreement? Find(string code);
    void Save(SkillProfileAgreement item);
    bool Remove(string code);
    List<SkillProfileAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfileAgreementStore(Dictionary<string, SkillProfileAgreement> items, List<string> order) : ISkillProfileAgreementStore
{
    public Dictionary<string, SkillProfileAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfileAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfileAgreement item)
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

    public List<SkillProfileAgreement> List()
    {
        var outValue = new List<SkillProfileAgreement>();
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
public sealed class CachedSkillProfileAgreementStore(MemorySkillProfileAgreementStore inner, SkillProfileAgreementCache cache, int hits, int misses) : ISkillProfileAgreementStore
{
    public MemorySkillProfileAgreementStore Inner { get; set; } = inner;
    public SkillProfileAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfileAgreement? Find(string code)
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

    public void Save(SkillProfileAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfileAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile agreements. Empty fields match all.</summary>
public sealed record SkillProfileAgreementQuery(string Title, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfileAgreement item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SkillProfileAgreement> Apply(List<SkillProfileAgreement> items)
    {
        var matched = Collections.Filter(items, (SkillProfileAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile agreement store.</summary>
public static class SkillProfileAgreementStore
{
    public static ISkillProfileAgreementStore NewMemorySkillProfileAgreementStore()
    {
        return new MemorySkillProfileAgreementStore(
            new Dictionary<string, SkillProfileAgreement>(),
            new List<string>());
    }

    public static ISkillProfileAgreementStore NewCachedSkillProfileAgreementStore(MemorySkillProfileAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfileAgreementStore(inner, SkillProfileAgreementCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SkillProfileAgreement> FindSkillProfileAgreements(ISkillProfileAgreementStore store, SkillProfileAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
