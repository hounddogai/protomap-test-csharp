namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work manifest is in its life cycle.</summary>
public enum StatementOfWorkManifestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a statement of work manifest and its current status.</summary>
public sealed class StatementOfWorkManifest(string code, string title, double budget, int limit, int endsAt, string summary, string source, string salaryBand, double maximum, int durationMinutes, StatementOfWorkManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Budget { get; set; } = budget;
    public int Limit { get; set; } = limit;
    public int EndsAt { get; set; } = endsAt;
    public string Summary { get; set; } = summary;
    public string Source { get; set; } = source;
    public string SalaryBand { get; set; } = salaryBand;
    public double Maximum { get; set; } = maximum;
    public int DurationMinutes { get; set; } = durationMinutes;
    public StatementOfWorkManifestStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work manifest in status idle.</summary>
    public static StatementOfWorkManifest NewStatementOfWorkManifest(string code, string title)
    {
        return new StatementOfWorkManifest(
            code,
            title,
            0.0,
            0,
            0,
            "",
            "",
            "",
            0.0,
            0,
            StatementOfWorkManifestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkManifestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkManifestStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkManifestStatus.Idle:
                switch (next)
                {
                    case StatementOfWorkManifestStatus.Ready:
                    case StatementOfWorkManifestStatus.Draining:
                        return true;
                }
                return false;
            case StatementOfWorkManifestStatus.Warming:
                return next == StatementOfWorkManifestStatus.Ready || next == StatementOfWorkManifestStatus.Draining;
            case StatementOfWorkManifestStatus.Ready:
                return next == StatementOfWorkManifestStatus.Draining;
            case StatementOfWorkManifestStatus.Draining:
                return next == StatementOfWorkManifestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Budget + (double)EndsAt, 2);
    }

    public StatementOfWorkManifest Copy()
    {
        return new StatementOfWorkManifest(
            Code,
            Title,
            Budget,
            Limit,
            EndsAt,
            Summary,
            Source,
            SalaryBand,
            Maximum,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
