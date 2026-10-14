namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped statement of work profiles by code.</summary>
public interface ICappedStatementOfWorkProfileStore
{
    CappedStatementOfWorkProfile? Find(string code);
    void Save(CappedStatementOfWorkProfile item);
    bool Remove(string code);
    List<CappedStatementOfWorkProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedStatementOfWorkProfileStore(Dictionary<string, CappedStatementOfWorkProfile> items, List<string> order) : ICappedStatementOfWorkProfileStore
{
    public Dictionary<string, CappedStatementOfWorkProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedStatementOfWorkProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedStatementOfWorkProfile item)
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

    public List<CappedStatementOfWorkProfile> List()
    {
        var outValue = new List<CappedStatementOfWorkProfile>();
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
public sealed class CachedCappedStatementOfWorkProfileStore(MemoryCappedStatementOfWorkProfileStore inner, CappedStatementOfWorkProfileCache cache, int hits, int misses) : ICappedStatementOfWorkProfileStore
{
    public MemoryCappedStatementOfWorkProfileStore Inner { get; set; } = inner;
    public CappedStatementOfWorkProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedStatementOfWorkProfile? Find(string code)
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

    public void Save(CappedStatementOfWorkProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedStatementOfWorkProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped statement of work profiles. Empty fields match all.</summary>
public sealed record CappedStatementOfWorkProfileQuery(string Department, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(CappedStatementOfWorkProfile item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<CappedStatementOfWorkProfile> Apply(List<CappedStatementOfWorkProfile> items)
    {
        var matched = Collections.Filter(items, (CappedStatementOfWorkProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped statement of work profile store.</summary>
public static class CappedStatementOfWorkProfileStore
{
    public static ICappedStatementOfWorkProfileStore NewMemoryCappedStatementOfWorkProfileStore()
    {
        return new MemoryCappedStatementOfWorkProfileStore(
            new Dictionary<string, CappedStatementOfWorkProfile>(),
            new List<string>());
    }

    public static ICappedStatementOfWorkProfileStore NewCachedCappedStatementOfWorkProfileStore(MemoryCappedStatementOfWorkProfileStore inner, int ttl, IClock clock)
    {
        return new CachedCappedStatementOfWorkProfileStore(
            inner,
            CappedStatementOfWorkProfileCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CappedStatementOfWorkProfile> FindCappedStatementOfWorkProfiles(ICappedStatementOfWorkProfileStore store, CappedStatementOfWorkProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
