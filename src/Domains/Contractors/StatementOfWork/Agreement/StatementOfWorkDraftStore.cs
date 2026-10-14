namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work drafts by code.</summary>
public interface IStatementOfWorkDraftStore
{
    StatementOfWorkDraft? Find(string code);
    void Save(StatementOfWorkDraft item);
    bool Remove(string code);
    List<StatementOfWorkDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkDraftStore(Dictionary<string, StatementOfWorkDraft> items, List<string> order) : IStatementOfWorkDraftStore
{
    public Dictionary<string, StatementOfWorkDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkDraft item)
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

    public List<StatementOfWorkDraft> List()
    {
        var outValue = new List<StatementOfWorkDraft>();
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
public sealed class CachedStatementOfWorkDraftStore(MemoryStatementOfWorkDraftStore inner, StatementOfWorkDraftCache cache, int hits, int misses) : IStatementOfWorkDraftStore
{
    public MemoryStatementOfWorkDraftStore Inner { get; set; } = inner;
    public StatementOfWorkDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkDraft? Find(string code)
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

    public void Save(StatementOfWorkDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work drafts. Empty fields match all.</summary>
public sealed record StatementOfWorkDraftQuery(string Segment, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkDraft item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkDraft> Apply(List<StatementOfWorkDraft> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work draft store.</summary>
public static class StatementOfWorkDraftStore
{
    public static IStatementOfWorkDraftStore NewMemoryStatementOfWorkDraftStore()
    {
        return new MemoryStatementOfWorkDraftStore(
            new Dictionary<string, StatementOfWorkDraft>(),
            new List<string>());
    }

    public static IStatementOfWorkDraftStore NewCachedStatementOfWorkDraftStore(MemoryStatementOfWorkDraftStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkDraftStore(inner, StatementOfWorkDraftCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<StatementOfWorkDraft> FindStatementOfWorkDrafts(IStatementOfWorkDraftStore store, StatementOfWorkDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
