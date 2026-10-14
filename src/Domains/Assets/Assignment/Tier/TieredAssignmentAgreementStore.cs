namespace Acme.Workforce.Assets.Assignment.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered assignment agreements by code.</summary>
public interface ITieredAssignmentAgreementStore
{
    TieredAssignmentAgreement? Find(string code);
    void Save(TieredAssignmentAgreement item);
    bool Remove(string code);
    List<TieredAssignmentAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredAssignmentAgreementStore(Dictionary<string, TieredAssignmentAgreement> items, List<string> order) : ITieredAssignmentAgreementStore
{
    public Dictionary<string, TieredAssignmentAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredAssignmentAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredAssignmentAgreement item)
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

    public List<TieredAssignmentAgreement> List()
    {
        var outValue = new List<TieredAssignmentAgreement>();
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
public sealed class CachedTieredAssignmentAgreementStore(MemoryTieredAssignmentAgreementStore inner, TieredAssignmentAgreementCache cache, int hits, int misses) : ITieredAssignmentAgreementStore
{
    public MemoryTieredAssignmentAgreementStore Inner { get; set; } = inner;
    public TieredAssignmentAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredAssignmentAgreement? Find(string code)
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

    public void Save(TieredAssignmentAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredAssignmentAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered assignment agreements. Empty fields match all.</summary>
public sealed record TieredAssignmentAgreementQuery(string Label, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(TieredAssignmentAgreement item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredAssignmentAgreement> Apply(List<TieredAssignmentAgreement> items)
    {
        var matched = Collections.Filter(items, (TieredAssignmentAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered assignment agreement store.</summary>
public static class TieredAssignmentAgreementStore
{
    public static ITieredAssignmentAgreementStore NewMemoryTieredAssignmentAgreementStore()
    {
        return new MemoryTieredAssignmentAgreementStore(
            new Dictionary<string, TieredAssignmentAgreement>(),
            new List<string>());
    }

    public static ITieredAssignmentAgreementStore NewCachedTieredAssignmentAgreementStore(MemoryTieredAssignmentAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedTieredAssignmentAgreementStore(
            inner,
            TieredAssignmentAgreementCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<TieredAssignmentAgreement> FindTieredAssignmentAgreements(ITieredAssignmentAgreementStore store, TieredAssignmentAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
