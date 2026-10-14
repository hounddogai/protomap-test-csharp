namespace Acme.Workforce.Contractors.Engagement.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched engagement exception is in its life cycle.</summary>
public enum BatchedEngagementExceptionStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One batched engagement exception, with what the planners need to act on it.</summary>
public sealed class BatchedEngagementException(string code, string label, double margin, string checksum, int windowDays, int tenureMonths, string name, double weight, string department, string team, BatchedEngagementExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public string Checksum { get; set; } = checksum;
    public int WindowDays { get; set; } = windowDays;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public string Department { get; set; } = department;
    public string Team { get; set; } = team;
    public BatchedEngagementExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a batched engagement exception in status draft.</summary>
    public static BatchedEngagementException NewBatchedEngagementException(string code, string label)
    {
        return new BatchedEngagementException(
            code,
            label,
            0.0,
            "",
            0,
            0,
            "",
            0.0,
            "",
            "",
            BatchedEngagementExceptionStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedEngagementExceptionStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedEngagementExceptionStatus next)
    {
        switch (Status)
        {
            case BatchedEngagementExceptionStatus.Draft:
                switch (next)
                {
                    case BatchedEngagementExceptionStatus.Active:
                        return true;
                }
                return false;
            case BatchedEngagementExceptionStatus.Pending:
                return next == BatchedEngagementExceptionStatus.Suspended || next == BatchedEngagementExceptionStatus.Closed;
            case BatchedEngagementExceptionStatus.Active:
                switch (next)
                {
                    case BatchedEngagementExceptionStatus.Suspended:
                    case BatchedEngagementExceptionStatus.Closed:
                        return true;
                }
                return false;
            case BatchedEngagementExceptionStatus.Suspended:
                return next == BatchedEngagementExceptionStatus.Closed;
        }
        return false;
    }

    public void MoveTo(BatchedEngagementExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched engagement exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Margin * (double)WindowDays, 2);
    }

    public BatchedEngagementException Copy()
    {
        return new BatchedEngagementException(
            Code,
            Label,
            Margin,
            Checksum,
            WindowDays,
            TenureMonths,
            Name,
            Weight,
            Department,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
