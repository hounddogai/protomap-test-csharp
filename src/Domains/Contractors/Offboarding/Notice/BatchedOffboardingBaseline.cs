namespace Acme.Workforce.Contractors.Offboarding.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched offboarding baseline is in its life cycle.</summary>
public enum BatchedOffboardingBaselineStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One batched offboarding baseline, with what the planners need to act on it.</summary>
public sealed class BatchedOffboardingBaseline(string code, string label, int endsAt, string department, string managerId, string locale, double floorValue, string category, string notes, BatchedOffboardingBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int EndsAt { get; set; } = endsAt;
    public string Department { get; set; } = department;
    public string ManagerId { get; set; } = managerId;
    public string Locale { get; set; } = locale;
    public double FloorValue { get; set; } = floorValue;
    public string Category { get; set; } = category;
    public string Notes { get; set; } = notes;
    public BatchedOffboardingBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a batched offboarding baseline in status draft.</summary>
    public static BatchedOffboardingBaseline NewBatchedOffboardingBaseline(string code, string label)
    {
        return new BatchedOffboardingBaseline(
            code,
            label,
            0,
            "",
            "",
            "",
            0.0,
            "",
            "",
            BatchedOffboardingBaselineStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedOffboardingBaselineStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedOffboardingBaselineStatus next)
    {
        switch (Status)
        {
            case BatchedOffboardingBaselineStatus.Draft:
                return next == BatchedOffboardingBaselineStatus.Pending || next == BatchedOffboardingBaselineStatus.Closed;
            case BatchedOffboardingBaselineStatus.Pending:
                switch (next)
                {
                    case BatchedOffboardingBaselineStatus.Suspended:
                        return true;
                }
                return false;
            case BatchedOffboardingBaselineStatus.Active:
                return next == BatchedOffboardingBaselineStatus.Suspended || next == BatchedOffboardingBaselineStatus.Closed;
            case BatchedOffboardingBaselineStatus.Suspended:
                return next == BatchedOffboardingBaselineStatus.Closed;
        }
        return false;
    }

    public void MoveTo(BatchedOffboardingBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched offboarding baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedExposure()
    {
        return Math.Round(FloorValue * (double)EndsAt, 2);
    }

    public BatchedOffboardingBaseline Copy()
    {
        return new BatchedOffboardingBaseline(
            Code,
            Label,
            EndsAt,
            Department,
            ManagerId,
            Locale,
            FloorValue,
            Category,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
