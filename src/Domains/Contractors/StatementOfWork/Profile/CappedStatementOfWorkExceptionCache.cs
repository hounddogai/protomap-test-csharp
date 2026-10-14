namespace Acme.Workforce.Contractors.StatementOfWork.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>CappedStatementOfWorkExceptionCache keeps CappedStatementOfWorkException records, as TtlCache keeps
/// any.</summary>
public sealed class CappedStatementOfWorkExceptionCache(Dictionary<string, CappedStatementOfWorkException> entries, Dictionary<string, int> expires, List<string> order, int capacity, int ttl, IClock clock)
{
    public Dictionary<string, CappedStatementOfWorkException> Entries { get; set; } = entries;
    public Dictionary<string, int> Expires { get; set; } = expires;
    public List<string> Order { get; set; } = order;
    public int Capacity { get; set; } = capacity;
    public int Ttl { get; set; } = ttl;
    public IClock Clock { get; set; } = clock;

    public static CappedStatementOfWorkExceptionCache Create(int capacity, int ttl, IClock clock)
    {
        return new CappedStatementOfWorkExceptionCache(
            new Dictionary<string, CappedStatementOfWorkException>(),
            new Dictionary<string, int>(),
            new List<string>(),
            capacity,
            ttl,
            clock);
    }

    public CappedStatementOfWorkException? Get(string key)
    {
        if (!Expires.TryGetValue(key, out var deadline))
        {
            return default;
        }
        if (Clock.Now() > deadline)
        {
            Entries.Remove(key);
            Expires.Remove(key);
            return default;
        }
        if (!Entries.TryGetValue(key, out var valueValue))
        {
            return default;
        }
        return valueValue;
    }

    public void Put(string key, CappedStatementOfWorkException valueValue)
    {
        if (!Entries.ContainsKey(key))
        {
            Order.Add(key);
        }
        Entries[key] = valueValue;
        Expires[key] = Clock.Now() + Ttl;
        while (Order.Count > Capacity && Order.Count > 0)
        {
            var oldest = Order[0];
            Order = Order.Skip(1).ToList();
            Entries.Remove(oldest);
            Expires.Remove(oldest);
        }
    }

    public int Size()
    {
        return Entries.Count;
    }

    public int PurgeExpired()
    {
        var now = Clock.Now();
        var kept = new List<string>();
        var removed = 0;
        foreach (var key in Order)
        {
            var deadline = Expires.GetValueOrDefault(key);
            if (now > deadline)
            {
                Entries.Remove(key);
                Expires.Remove(key);
                removed += 1;
                continue;
            }
            kept.Add(key);
        }
        Order = kept;
        return removed;
    }
}
