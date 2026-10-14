namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work profiles by code.</summary>
public interface IStatementOfWorkProfileStore
{
    StatementOfWorkProfile? Find(string code);
    void Save(StatementOfWorkProfile item);
    bool Remove(string code);
    List<StatementOfWorkProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkProfileStore(Dictionary<string, StatementOfWorkProfile> items, List<string> order) : IStatementOfWorkProfileStore
{
    public Dictionary<string, StatementOfWorkProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkProfile item)
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

    public List<StatementOfWorkProfile> List()
    {
        var outValue = new List<StatementOfWorkProfile>();
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
public sealed class CachedStatementOfWorkProfileStore(MemoryStatementOfWorkProfileStore inner, StatementOfWorkProfileCache cache, int hits, int misses) : IStatementOfWorkProfileStore
{
    public MemoryStatementOfWorkProfileStore Inner { get; set; } = inner;
    public StatementOfWorkProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkProfile? Find(string code)
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

    public void Save(StatementOfWorkProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work profiles. Empty fields match all.</summary>
public sealed record StatementOfWorkProfileQuery(string Label, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkProfile item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<StatementOfWorkProfile> Apply(List<StatementOfWorkProfile> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work profile store.</summary>
public static class StatementOfWorkProfileStore
{
    public static IStatementOfWorkProfileStore NewMemoryStatementOfWorkProfileStore()
    {
        return new MemoryStatementOfWorkProfileStore(
            new Dictionary<string, StatementOfWorkProfile>(),
            new List<string>());
    }

    public static IStatementOfWorkProfileStore NewCachedStatementOfWorkProfileStore(MemoryStatementOfWorkProfileStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkProfileStore(
            inner,
            StatementOfWorkProfileCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<StatementOfWorkProfile> FindStatementOfWorkProfiles(IStatementOfWorkProfileStore store, StatementOfWorkProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
