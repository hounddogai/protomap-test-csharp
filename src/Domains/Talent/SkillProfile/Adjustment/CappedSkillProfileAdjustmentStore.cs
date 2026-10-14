namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped skill profile adjustments by code.</summary>
public interface ICappedSkillProfileAdjustmentStore
{
    CappedSkillProfileAdjustment? Find(string code);
    void Save(CappedSkillProfileAdjustment item);
    bool Remove(string code);
    List<CappedSkillProfileAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedSkillProfileAdjustmentStore(Dictionary<string, CappedSkillProfileAdjustment> items, List<string> order) : ICappedSkillProfileAdjustmentStore
{
    public Dictionary<string, CappedSkillProfileAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedSkillProfileAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedSkillProfileAdjustment item)
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

    public List<CappedSkillProfileAdjustment> List()
    {
        var outValue = new List<CappedSkillProfileAdjustment>();
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
public sealed class CachedCappedSkillProfileAdjustmentStore(MemoryCappedSkillProfileAdjustmentStore inner, CappedSkillProfileAdjustmentCache cache, int hits, int misses) : ICappedSkillProfileAdjustmentStore
{
    public MemoryCappedSkillProfileAdjustmentStore Inner { get; set; } = inner;
    public CappedSkillProfileAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedSkillProfileAdjustment? Find(string code)
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

    public void Save(CappedSkillProfileAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedSkillProfileAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped skill profile adjustments. Empty fields match all.</summary>
public sealed record CappedSkillProfileAdjustmentQuery(string Segment, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(CappedSkillProfileAdjustment item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedSkillProfileAdjustment> Apply(List<CappedSkillProfileAdjustment> items)
    {
        var matched = Collections.Filter(items, (CappedSkillProfileAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped skill profile adjustment store.</summary>
public static class CappedSkillProfileAdjustmentStore
{
    public static ICappedSkillProfileAdjustmentStore NewMemoryCappedSkillProfileAdjustmentStore()
    {
        return new MemoryCappedSkillProfileAdjustmentStore(
            new Dictionary<string, CappedSkillProfileAdjustment>(),
            new List<string>());
    }

    public static ICappedSkillProfileAdjustmentStore NewCachedCappedSkillProfileAdjustmentStore(MemoryCappedSkillProfileAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedCappedSkillProfileAdjustmentStore(
            inner,
            CappedSkillProfileAdjustmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CappedSkillProfileAdjustment> FindCappedSkillProfileAdjustments(ICappedSkillProfileAdjustmentStore store, CappedSkillProfileAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
