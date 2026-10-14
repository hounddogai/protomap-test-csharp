namespace Acme.Workforce.Absence.Balance.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive balance summary is in its life cycle.</summary>
public enum AdaptiveBalanceSummaryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A adaptive balance summary. Its code is unique within a tenant.</summary>
public sealed class AdaptiveBalanceSummary(string code, string label, string team, int leaveDays, bool archived, int batchSize, string contactEmail, int attempts, string timezone, string employeeId, string name, AdaptiveBalanceSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public int LeaveDays { get; set; } = leaveDays;
    public bool Archived { get; set; } = archived;
    public int BatchSize { get; set; } = batchSize;
    public string ContactEmail { get; set; } = contactEmail;
    public int Attempts { get; set; } = attempts;
    public string Timezone { get; set; } = timezone;
    public string EmployeeId { get; set; } = employeeId;
    public string Name { get; set; } = name;
    public AdaptiveBalanceSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive balance summary in status requested.</summary>
    public static AdaptiveBalanceSummary NewAdaptiveBalanceSummary(string code, string label)
    {
        return new AdaptiveBalanceSummary(
            code,
            label,
            "",
            0,
            false,
            0,
            "",
            0,
            "",
            "",
            "",
            AdaptiveBalanceSummaryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveBalanceSummaryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveBalanceSummaryStatus next)
    {
        switch (Status)
        {
            case AdaptiveBalanceSummaryStatus.Requested:
                return next == AdaptiveBalanceSummaryStatus.Granted || next == AdaptiveBalanceSummaryStatus.Revoked;
            case AdaptiveBalanceSummaryStatus.Granted:
                return next == AdaptiveBalanceSummaryStatus.Expired;
            case AdaptiveBalanceSummaryStatus.Revoked:
                switch (next)
                {
                    case AdaptiveBalanceSummaryStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveBalanceSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive balance summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)LeaveDays * (double)Attempts, 2);
    }

    public AdaptiveBalanceSummary Copy()
    {
        return new AdaptiveBalanceSummary(
            Code,
            Label,
            Team,
            LeaveDays,
            Archived,
            BatchSize,
            ContactEmail,
            Attempts,
            Timezone,
            EmployeeId,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
