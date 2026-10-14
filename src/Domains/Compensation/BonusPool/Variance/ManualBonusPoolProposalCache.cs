namespace Acme.Workforce.Compensation.BonusPool.Variance;

using Acme.Workforce.Kit.Clock;

/// <summary>ManualBonusPoolProposalCache keeps ManualBonusPoolProposal records, as TtlCache keeps any.</summary>
public sealed class ManualBonusPoolProposalCache(Dictionary<string, ManualBonusPoolProposal> entries, Dictionary<string, int> expires, List<string> order, int capacity, int ttl, IClock clock)
{
    public Dictionary<string, ManualBonusPoolProposal> Entries { get; set; } = entries;
    public Dictionary<string, int> Expires { get; set; } = expires;
    public List<string> Order { get; set; } = order;
    public int Capacity { get; set; } = capacity;
    public int Ttl { get; set; } = ttl;
    public IClock Clock { get; set; } = clock;

    public static ManualBonusPoolProposalCache Create(int capacity, int ttl, IClock clock)
    {
        return new ManualBonusPoolProposalCache(
            new Dictionary<string, ManualBonusPoolProposal>(),
            new Dictionary<string, int>(),
            new List<string>(),
            capacity,
            ttl,
            clock);
    }

    public ManualBonusPoolProposal? Get(string key)
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

    public void Put(string key, ManualBonusPoolProposal valueValue)
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
