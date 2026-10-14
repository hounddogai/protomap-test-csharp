namespace Acme.Workforce.Talent.Pool.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool policy is in its life cycle.</summary>
public enum PoolPolicyStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a pool policy and its current status.</summary>
public sealed class PoolPolicy(string code, string name, string department, Dictionary<string, string> attributes, string ownerId, int capacity, PoolPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string OwnerId { get; set; } = ownerId;
    public int Capacity { get; set; } = capacity;
    public PoolPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a pool policy in status created.</summary>
    public static PoolPolicy NewPoolPolicy(string code, string name)
    {
        return new PoolPolicy(code, name, "", new Dictionary<string, string>(), "", 0, PoolPolicyStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolPolicyStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolPolicyStatus next)
    {
        switch (Status)
        {
            case PoolPolicyStatus.Created:
                return next == PoolPolicyStatus.Published || next == PoolPolicyStatus.Retired;
            case PoolPolicyStatus.Validated:
                return next == PoolPolicyStatus.Published;
            case PoolPolicyStatus.Published:
                switch (next)
                {
                    case PoolPolicyStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double MeasuredValue()
    {
        return (double)Capacity;
    }

    public PoolPolicy Copy()
    {
        return new PoolPolicy(Code, Name, Department, Attributes, OwnerId, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
