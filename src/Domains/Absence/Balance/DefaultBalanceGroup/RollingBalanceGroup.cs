namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling balance group is in its life cycle.</summary>
public enum RollingBalanceGroupStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a rolling balance group and its current status.</summary>
public sealed class RollingBalanceGroup(string code, string label, double ratio, int sequence, List<string> tags, int updatedAt, RollingBalanceGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public int Sequence { get; set; } = sequence;
    public List<string> Tags { get; set; } = tags;
    public int UpdatedAt { get; set; } = updatedAt;
    public RollingBalanceGroupStatus Status { get; set; } = status;

    /// <summary>Starts a rolling balance group in status requested.</summary>
    public static RollingBalanceGroup NewRollingBalanceGroup(string code, string label)
    {
        return new RollingBalanceGroup(
            code,
            label,
            0.0,
            0,
            new List<string>(),
            0,
            RollingBalanceGroupStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingBalanceGroupStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingBalanceGroupStatus next)
    {
        switch (Status)
        {
            case RollingBalanceGroupStatus.Requested:
                switch (next)
                {
                    case RollingBalanceGroupStatus.Expired:
                        return true;
                }
                return false;
            case RollingBalanceGroupStatus.Granted:
                switch (next)
                {
                    case RollingBalanceGroupStatus.Revoked:
                        return true;
                }
                return false;
            case RollingBalanceGroupStatus.Revoked:
                switch (next)
                {
                    case RollingBalanceGroupStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingBalanceGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling balance group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
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

    public double ProjectedExposure()
    {
        return Math.Round((double)Sequence + Ratio, 2);
    }

    public RollingBalanceGroup Copy()
    {
        return new RollingBalanceGroup(Code, Label, Ratio, Sequence, Tags, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
