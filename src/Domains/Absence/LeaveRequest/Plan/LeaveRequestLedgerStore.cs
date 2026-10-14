namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request ledgers by code.</summary>
public interface ILeaveRequestLedgerStore
{
    LeaveRequestLedger? Find(string code);
    void Save(LeaveRequestLedger item);
    bool Remove(string code);
    List<LeaveRequestLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestLedgerStore(Dictionary<string, LeaveRequestLedger> items, List<string> order) : ILeaveRequestLedgerStore
{
    public Dictionary<string, LeaveRequestLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestLedger item)
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

    public List<LeaveRequestLedger> List()
    {
        var outValue = new List<LeaveRequestLedger>();
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
public sealed class CachedLeaveRequestLedgerStore(MemoryLeaveRequestLedgerStore inner, LeaveRequestLedgerCache cache, int hits, int misses) : ILeaveRequestLedgerStore
{
    public MemoryLeaveRequestLedgerStore Inner { get; set; } = inner;
    public LeaveRequestLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestLedger? Find(string code)
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

    public void Save(LeaveRequestLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request ledgers. Empty fields match all.</summary>
public sealed record LeaveRequestLedgerQuery(string Team, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestLedger item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<LeaveRequestLedger> Apply(List<LeaveRequestLedger> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request ledger store.</summary>
public static class LeaveRequestLedgerStore
{
    public static ILeaveRequestLedgerStore NewMemoryLeaveRequestLedgerStore()
    {
        return new MemoryLeaveRequestLedgerStore(new Dictionary<string, LeaveRequestLedger>(), new List<string>());
    }

    public static ILeaveRequestLedgerStore NewCachedLeaveRequestLedgerStore(MemoryLeaveRequestLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestLedgerStore(inner, LeaveRequestLedgerCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestLedger> FindLeaveRequestLedgers(ILeaveRequestLedgerStore store, LeaveRequestLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
