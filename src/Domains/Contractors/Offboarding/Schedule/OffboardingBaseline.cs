namespace Acme.Workforce.Contractors.Offboarding.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding baseline is in its life cycle.</summary>
public enum OffboardingBaselineStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A offboarding baseline. Its code is unique within a tenant.</summary>
public sealed class OffboardingBaseline(string code, string title, bool approved, int createdAt, int limit, int sequence, double rate, string label, double minimum, OffboardingBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public int CreatedAt { get; set; } = createdAt;
    public int Limit { get; set; } = limit;
    public int Sequence { get; set; } = sequence;
    public double Rate { get; set; } = rate;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public OffboardingBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding baseline in status requested.</summary>
    public static OffboardingBaseline NewOffboardingBaseline(string code, string title)
    {
        return new OffboardingBaseline(
            code,
            title,
            false,
            0,
            0,
            0,
            0.0,
            "",
            0.0,
            OffboardingBaselineStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingBaselineStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingBaselineStatus next)
    {
        switch (Status)
        {
            case OffboardingBaselineStatus.Requested:
                switch (next)
                {
                    case OffboardingBaselineStatus.Revoked:
                    case OffboardingBaselineStatus.Expired:
                        return true;
                }
                return false;
            case OffboardingBaselineStatus.Granted:
                return next == OffboardingBaselineStatus.Revoked || next == OffboardingBaselineStatus.Expired;
            case OffboardingBaselineStatus.Revoked:
                switch (next)
                {
                    case OffboardingBaselineStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OffboardingBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Title}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Sequence * (double)CreatedAt, 2);
    }

    public OffboardingBaseline Copy()
    {
        return new OffboardingBaseline(
            Code,
            Title,
            Approved,
            CreatedAt,
            Limit,
            Sequence,
            Rate,
            Label,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
