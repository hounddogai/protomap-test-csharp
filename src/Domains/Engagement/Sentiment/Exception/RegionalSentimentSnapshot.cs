namespace Acme.Workforce.Engagement.Sentiment.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional sentiment snapshot is in its life cycle.</summary>
public enum RegionalSentimentSnapshotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A regional sentiment snapshot. Its code is unique within a tenant.</summary>
public sealed class RegionalSentimentSnapshot(string code, string label, string employeeId, string summary, bool pinned, double minimum, int attempts, string salaryBand, RegionalSentimentSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public string Summary { get; set; } = summary;
    public bool Pinned { get; set; } = pinned;
    public double Minimum { get; set; } = minimum;
    public int Attempts { get; set; } = attempts;
    public string SalaryBand { get; set; } = salaryBand;
    public RegionalSentimentSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a regional sentiment snapshot in status open.</summary>
    public static RegionalSentimentSnapshot NewRegionalSentimentSnapshot(string code, string label)
    {
        return new RegionalSentimentSnapshot(
            code,
            label,
            "",
            "",
            false,
            0.0,
            0,
            "",
            RegionalSentimentSnapshotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalSentimentSnapshotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalSentimentSnapshotStatus next)
    {
        switch (Status)
        {
            case RegionalSentimentSnapshotStatus.Open:
                switch (next)
                {
                    case RegionalSentimentSnapshotStatus.InReview:
                    case RegionalSentimentSnapshotStatus.Archived:
                        return true;
                }
                return false;
            case RegionalSentimentSnapshotStatus.InReview:
                switch (next)
                {
                    case RegionalSentimentSnapshotStatus.Reopened:
                    case RegionalSentimentSnapshotStatus.Archived:
                        return true;
                }
                return false;
            case RegionalSentimentSnapshotStatus.Resolved:
                return next == RegionalSentimentSnapshotStatus.Reopened;
            case RegionalSentimentSnapshotStatus.Reopened:
                return next == RegionalSentimentSnapshotStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RegionalSentimentSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional sentiment snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"regional sentiment snapshot"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Minimum * (double)Attempts, 2);
    }

    public RegionalSentimentSnapshot Copy()
    {
        return new RegionalSentimentSnapshot(
            Code,
            Label,
            EmployeeId,
            Summary,
            Pinned,
            Minimum,
            Attempts,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
