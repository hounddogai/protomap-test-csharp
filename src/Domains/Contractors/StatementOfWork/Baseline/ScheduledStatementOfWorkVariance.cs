namespace Acme.Workforce.Contractors.StatementOfWork.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled statement of work variance is in its life cycle.</summary>
public enum ScheduledStatementOfWorkVarianceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One scheduled statement of work variance, with what the planners need to act on it.</summary>
public sealed class ScheduledStatementOfWorkVariance(string code, string name, string grade, int startsAt, double weight, string region, string timezone, int updatedAt, string segment, ScheduledStatementOfWorkVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public int StartsAt { get; set; } = startsAt;
    public double Weight { get; set; } = weight;
    public string Region { get; set; } = region;
    public string Timezone { get; set; } = timezone;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Segment { get; set; } = segment;
    public ScheduledStatementOfWorkVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled statement of work variance in status created.</summary>
    public static ScheduledStatementOfWorkVariance NewScheduledStatementOfWorkVariance(string code, string name)
    {
        return new ScheduledStatementOfWorkVariance(
            code,
            name,
            "",
            0,
            0.0,
            "",
            "",
            0,
            "",
            ScheduledStatementOfWorkVarianceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledStatementOfWorkVarianceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledStatementOfWorkVarianceStatus next)
    {
        switch (Status)
        {
            case ScheduledStatementOfWorkVarianceStatus.Created:
                return next == ScheduledStatementOfWorkVarianceStatus.Retired;
            case ScheduledStatementOfWorkVarianceStatus.Validated:
                return next == ScheduledStatementOfWorkVarianceStatus.Published || next == ScheduledStatementOfWorkVarianceStatus.Retired;
            case ScheduledStatementOfWorkVarianceStatus.Published:
                switch (next)
                {
                    case ScheduledStatementOfWorkVarianceStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledStatementOfWorkVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled statement of work variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Weight * (double)StartsAt, 2);
    }

    public ScheduledStatementOfWorkVariance Copy()
    {
        return new ScheduledStatementOfWorkVariance(
            Code,
            Name,
            Grade,
            StartsAt,
            Weight,
            Region,
            Timezone,
            UpdatedAt,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
