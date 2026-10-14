namespace Acme.Workforce.Absence.Balance.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback balance cycle is in its life cycle.</summary>
public enum FallbackBalanceCycleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a fallback balance cycle and its current status.</summary>
public sealed class FallbackBalanceCycle(string code, string label, bool active, string checksum, int createdAt, Dictionary<string, string> attributes, int attempts, string phone, double amount, int version, double ratio, FallbackBalanceCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Active { get; set; } = active;
    public string Checksum { get; set; } = checksum;
    public int CreatedAt { get; set; } = createdAt;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Attempts { get; set; } = attempts;
    public string Phone { get; set; } = phone;
    public double Amount { get; set; } = amount;
    public int Version { get; set; } = version;
    public double Ratio { get; set; } = ratio;
    public FallbackBalanceCycleStatus Status { get; set; } = status;

    /// <summary>Starts a fallback balance cycle in status proposed.</summary>
    public static FallbackBalanceCycle NewFallbackBalanceCycle(string code, string label)
    {
        return new FallbackBalanceCycle(
            code,
            label,
            false,
            "",
            0,
            new Dictionary<string, string>(),
            0,
            "",
            0.0,
            0,
            0.0,
            FallbackBalanceCycleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackBalanceCycleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackBalanceCycleStatus next)
    {
        switch (Status)
        {
            case FallbackBalanceCycleStatus.Proposed:
                return next == FallbackBalanceCycleStatus.Approved || next == FallbackBalanceCycleStatus.Withdrawn;
            case FallbackBalanceCycleStatus.Approved:
                switch (next)
                {
                    case FallbackBalanceCycleStatus.Rejected:
                    case FallbackBalanceCycleStatus.Withdrawn:
                        return true;
                }
                return false;
            case FallbackBalanceCycleStatus.Rejected:
                switch (next)
                {
                    case FallbackBalanceCycleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackBalanceCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback balance cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"fallback balance cycle"} {Code}";
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

    public double NetTotal()
    {
        return Math.Round((double)Version * (double)CreatedAt, 2);
    }

    public FallbackBalanceCycle Copy()
    {
        return new FallbackBalanceCycle(
            Code,
            Label,
            Active,
            Checksum,
            CreatedAt,
            Attributes,
            Attempts,
            Phone,
            Amount,
            Version,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
