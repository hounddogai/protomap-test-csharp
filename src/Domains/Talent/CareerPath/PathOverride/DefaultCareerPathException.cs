namespace Acme.Workforce.Talent.CareerPath.PathOverride;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default career path exception is in its life cycle.</summary>
public enum DefaultCareerPathExceptionStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A default career path exception. Its code is unique within a tenant.</summary>
public sealed class DefaultCareerPathException(string code, string label, string salaryBand, string segment, int startsAt, string reviewer, string ownerId, string name, int sequence, DefaultCareerPathExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public string Segment { get; set; } = segment;
    public int StartsAt { get; set; } = startsAt;
    public string Reviewer { get; set; } = reviewer;
    public string OwnerId { get; set; } = ownerId;
    public string Name { get; set; } = name;
    public int Sequence { get; set; } = sequence;
    public DefaultCareerPathExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a default career path exception in status draft.</summary>
    public static DefaultCareerPathException NewDefaultCareerPathException(string code, string label)
    {
        return new DefaultCareerPathException(
            code,
            label,
            "",
            "",
            0,
            "",
            "",
            "",
            0,
            DefaultCareerPathExceptionStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultCareerPathExceptionStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultCareerPathExceptionStatus next)
    {
        switch (Status)
        {
            case DefaultCareerPathExceptionStatus.Draft:
                return next == DefaultCareerPathExceptionStatus.Active;
            case DefaultCareerPathExceptionStatus.Pending:
                switch (next)
                {
                    case DefaultCareerPathExceptionStatus.Active:
                    case DefaultCareerPathExceptionStatus.Suspended:
                        return true;
                }
                return false;
            case DefaultCareerPathExceptionStatus.Active:
                return next == DefaultCareerPathExceptionStatus.Suspended || next == DefaultCareerPathExceptionStatus.Closed;
            case DefaultCareerPathExceptionStatus.Suspended:
                switch (next)
                {
                    case DefaultCareerPathExceptionStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultCareerPathExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default career path exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetExposure()
    {
        return Math.Round((double)StartsAt + (double)Sequence, 2);
    }

    public DefaultCareerPathException Copy()
    {
        return new DefaultCareerPathException(
            Code,
            Label,
            SalaryBand,
            Segment,
            StartsAt,
            Reviewer,
            OwnerId,
            Name,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
