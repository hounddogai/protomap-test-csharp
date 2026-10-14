namespace Acme.Workforce.Contractors.StatementOfWork.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped statement of work exceptions by code.</summary>
public interface ICappedStatementOfWorkExceptionStore
{
    CappedStatementOfWorkException? Find(string code);
    void Save(CappedStatementOfWorkException item);
    bool Remove(string code);
    List<CappedStatementOfWorkException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedStatementOfWorkExceptionStore(Dictionary<string, CappedStatementOfWorkException> items, List<string> order) : ICappedStatementOfWorkExceptionStore
{
    public Dictionary<string, CappedStatementOfWorkException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedStatementOfWorkException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedStatementOfWorkException item)
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

    public List<CappedStatementOfWorkException> List()
    {
        var outValue = new List<CappedStatementOfWorkException>();
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
public sealed class CachedCappedStatementOfWorkExceptionStore(MemoryCappedStatementOfWorkExceptionStore inner, CappedStatementOfWorkExceptionCache cache, int hits, int misses) : ICappedStatementOfWorkExceptionStore
{
    public MemoryCappedStatementOfWorkExceptionStore Inner { get; set; } = inner;
    public CappedStatementOfWorkExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedStatementOfWorkException? Find(string code)
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

    public void Save(CappedStatementOfWorkException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedStatementOfWorkException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped statement of work exceptions. Empty fields match all.</summary>
public sealed record CappedStatementOfWorkExceptionQuery(string Channel, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(CappedStatementOfWorkException item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedStatementOfWorkException> Apply(List<CappedStatementOfWorkException> items)
    {
        var matched = Collections.Filter(items, (CappedStatementOfWorkException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped statement of work exception store.</summary>
public static class CappedStatementOfWorkExceptionStore
{
    public static ICappedStatementOfWorkExceptionStore NewMemoryCappedStatementOfWorkExceptionStore()
    {
        return new MemoryCappedStatementOfWorkExceptionStore(
            new Dictionary<string, CappedStatementOfWorkException>(),
            new List<string>());
    }

    public static ICappedStatementOfWorkExceptionStore NewCachedCappedStatementOfWorkExceptionStore(MemoryCappedStatementOfWorkExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedCappedStatementOfWorkExceptionStore(
            inner,
            CappedStatementOfWorkExceptionCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<CappedStatementOfWorkException> FindCappedStatementOfWorkExceptions(ICappedStatementOfWorkExceptionStore store, CappedStatementOfWorkExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
