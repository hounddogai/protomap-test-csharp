namespace Acme.Workforce.Compensation.Adjustment.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment agreement is in its life cycle.</summary>
public enum AdjustmentAgreementStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One adjustment agreement, with what the planners need to act on it.</summary>
public sealed class AdjustmentAgreement(string code, string name, string country, double minimum, string team, bool pinned, string checksum, int createdAt, int retries, string grade, string summary, AdjustmentAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public double Minimum { get; set; } = minimum;
    public string Team { get; set; } = team;
    public bool Pinned { get; set; } = pinned;
    public string Checksum { get; set; } = checksum;
    public int CreatedAt { get; set; } = createdAt;
    public int Retries { get; set; } = retries;
    public string Grade { get; set; } = grade;
    public string Summary { get; set; } = summary;
    public AdjustmentAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment agreement in status idle.</summary>
    public static AdjustmentAgreement NewAdjustmentAgreement(string code, string name)
    {
        return new AdjustmentAgreement(
            code,
            name,
            "",
            0.0,
            "",
            false,
            "",
            0,
            0,
            "",
            "",
            AdjustmentAgreementStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentAgreementStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentAgreementStatus next)
    {
        switch (Status)
        {
            case AdjustmentAgreementStatus.Idle:
                switch (next)
                {
                    case AdjustmentAgreementStatus.Warming:
                    case AdjustmentAgreementStatus.Ready:
                        return true;
                }
                return false;
            case AdjustmentAgreementStatus.Warming:
                switch (next)
                {
                    case AdjustmentAgreementStatus.Stopped:
                        return true;
                }
                return false;
            case AdjustmentAgreementStatus.Ready:
                return next == AdjustmentAgreementStatus.Draining || next == AdjustmentAgreementStatus.Stopped;
            case AdjustmentAgreementStatus.Draining:
                switch (next)
                {
                    case AdjustmentAgreementStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdjustmentAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"adjustment agreement"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)CreatedAt - (double)Retries, 2);
    }

    public AdjustmentAgreement Copy()
    {
        return new AdjustmentAgreement(
            Code,
            Name,
            Country,
            Minimum,
            Team,
            Pinned,
            Checksum,
            CreatedAt,
            Retries,
            Grade,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
