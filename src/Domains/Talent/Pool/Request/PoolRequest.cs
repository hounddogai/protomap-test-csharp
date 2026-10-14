namespace Acme.Workforce.Talent.Pool.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool request is in its life cycle.</summary>
public enum PoolRequestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One pool request, with what the planners need to act on it.</summary>
public sealed class PoolRequest(string code, string name, int capacity, string currency, Dictionary<string, string> attributes, string costCenter, double discount, string contactEmail, List<string> tags, double cost, PoolRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public string Currency { get; set; } = currency;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string CostCenter { get; set; } = costCenter;
    public double Discount { get; set; } = discount;
    public string ContactEmail { get; set; } = contactEmail;
    public List<string> Tags { get; set; } = tags;
    public double Cost { get; set; } = cost;
    public PoolRequestStatus Status { get; set; } = status;

    /// <summary>Starts a pool request in status new.</summary>
    public static PoolRequest NewPoolRequest(string code, string name)
    {
        return new PoolRequest(
            code,
            name,
            0,
            "",
            new Dictionary<string, string>(),
            "",
            0.0,
            "",
            new List<string>(),
            0.0,
            PoolRequestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolRequestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolRequestStatus next)
    {
        switch (Status)
        {
            case PoolRequestStatus.New:
                return next == PoolRequestStatus.Running || next == PoolRequestStatus.Failed;
            case PoolRequestStatus.Queued:
                return next == PoolRequestStatus.Done || next == PoolRequestStatus.Failed;
            case PoolRequestStatus.Running:
                switch (next)
                {
                    case PoolRequestStatus.Done:
                    case PoolRequestStatus.Failed:
                        return true;
                }
                return false;
            case PoolRequestStatus.Done:
                switch (next)
                {
                    case PoolRequestStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pool request"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetLoad()
    {
        return Math.Round((double)Capacity + Discount, 2);
    }

    public PoolRequest Copy()
    {
        return new PoolRequest(
            Code,
            Name,
            Capacity,
            Currency,
            Attributes,
            CostCenter,
            Discount,
            ContactEmail,
            Tags,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
