namespace Acme.Workforce.Talent.CareerPath.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled career path profile is in its life cycle.</summary>
public enum ScheduledCareerPathProfileStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A scheduled career path profile. Its code is unique within a tenant.</summary>
public sealed class ScheduledCareerPathProfile(string code, string title, int updatedAt, int batchSize, int createdAt, double minimum, string country, string currency, ScheduledCareerPathProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public int BatchSize { get; set; } = batchSize;
    public int CreatedAt { get; set; } = createdAt;
    public double Minimum { get; set; } = minimum;
    public string Country { get; set; } = country;
    public string Currency { get; set; } = currency;
    public ScheduledCareerPathProfileStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled career path profile in status idle.</summary>
    public static ScheduledCareerPathProfile NewScheduledCareerPathProfile(string code, string title)
    {
        return new ScheduledCareerPathProfile(
            code,
            title,
            0,
            0,
            0,
            0.0,
            "",
            "",
            ScheduledCareerPathProfileStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledCareerPathProfileStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledCareerPathProfileStatus next)
    {
        switch (Status)
        {
            case ScheduledCareerPathProfileStatus.Idle:
                switch (next)
                {
                    case ScheduledCareerPathProfileStatus.Ready:
                        return true;
                }
                return false;
            case ScheduledCareerPathProfileStatus.Warming:
                switch (next)
                {
                    case ScheduledCareerPathProfileStatus.Stopped:
                        return true;
                }
                return false;
            case ScheduledCareerPathProfileStatus.Ready:
                return next == ScheduledCareerPathProfileStatus.Stopped;
            case ScheduledCareerPathProfileStatus.Draining:
                return next == ScheduledCareerPathProfileStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ScheduledCareerPathProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled career path profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)CreatedAt * (double)UpdatedAt, 2);
    }

    public ScheduledCareerPathProfile Copy()
    {
        return new ScheduledCareerPathProfile(
            Code,
            Title,
            UpdatedAt,
            BatchSize,
            CreatedAt,
            Minimum,
            Country,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
