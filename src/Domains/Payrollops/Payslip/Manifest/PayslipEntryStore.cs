namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip entries by code.</summary>
public interface IPayslipEntryStore
{
    PayslipEntry? Find(string code);
    void Save(PayslipEntry item);
    bool Remove(string code);
    List<PayslipEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipEntryStore(Dictionary<string, PayslipEntry> items, List<string> order) : IPayslipEntryStore
{
    public Dictionary<string, PayslipEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipEntry item)
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

    public List<PayslipEntry> List()
    {
        var outValue = new List<PayslipEntry>();
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
public sealed class CachedPayslipEntryStore(MemoryPayslipEntryStore inner, PayslipEntryCache cache, int hits, int misses) : IPayslipEntryStore
{
    public MemoryPayslipEntryStore Inner { get; set; } = inner;
    public PayslipEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipEntry? Find(string code)
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

    public void Save(PayslipEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip entries. Empty fields match all.</summary>
public sealed record PayslipEntryQuery(string Title, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipEntry item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayslipEntry> Apply(List<PayslipEntry> items)
    {
        var matched = Collections.Filter(items, (PayslipEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip entry store.</summary>
public static class PayslipEntryStore
{
    public static IPayslipEntryStore NewMemoryPayslipEntryStore()
    {
        return new MemoryPayslipEntryStore(new Dictionary<string, PayslipEntry>(), new List<string>());
    }

    public static IPayslipEntryStore NewCachedPayslipEntryStore(MemoryPayslipEntryStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipEntryStore(inner, PayslipEntryCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PayslipEntry> FindPayslipEntries(IPayslipEntryStore store, PayslipEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
