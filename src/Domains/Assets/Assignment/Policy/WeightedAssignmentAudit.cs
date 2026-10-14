namespace Acme.Workforce.Assets.Assignment.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted assignment audit is in its life cycle.</summary>
public enum WeightedAssignmentAuditStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A weighted assignment audit. Its code is unique within a tenant.</summary>
public sealed class WeightedAssignmentAudit(string code, string title, string salaryBand, double rating, int capacity, bool visible, double amount, int endsAt, WeightedAssignmentAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public double Rating { get; set; } = rating;
    public int Capacity { get; set; } = capacity;
    public bool Visible { get; set; } = visible;
    public double Amount { get; set; } = amount;
    public int EndsAt { get; set; } = endsAt;
    public WeightedAssignmentAuditStatus Status { get; set; } = status;

    /// <summary>Starts a weighted assignment audit in status planned.</summary>
    public static WeightedAssignmentAudit NewWeightedAssignmentAudit(string code, string title)
    {
        return new WeightedAssignmentAudit(
            code,
            title,
            "",
            0.0,
            0,
            false,
            0.0,
            0,
            WeightedAssignmentAuditStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAssignmentAuditStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAssignmentAuditStatus next)
    {
        switch (Status)
        {
            case WeightedAssignmentAuditStatus.Planned:
                return next == WeightedAssignmentAuditStatus.Scheduled;
            case WeightedAssignmentAuditStatus.Scheduled:
                return next == WeightedAssignmentAuditStatus.InProgress || next == WeightedAssignmentAuditStatus.Completed;
            case WeightedAssignmentAuditStatus.InProgress:
                switch (next)
                {
                    case WeightedAssignmentAuditStatus.Completed:
                    case WeightedAssignmentAuditStatus.Cancelled:
                        return true;
                }
                return false;
            case WeightedAssignmentAuditStatus.Completed:
                return next == WeightedAssignmentAuditStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(WeightedAssignmentAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted assignment audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"weighted assignment audit"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Capacity * Rating, 2);
    }

    public WeightedAssignmentAudit Copy()
    {
        return new WeightedAssignmentAudit(
            Code,
            Title,
            SalaryBand,
            Rating,
            Capacity,
            Visible,
            Amount,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
