namespace Acme.Workforce.Engagement.Recognition.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback recognition override is in its life cycle.</summary>
public enum FallbackRecognitionOverrideStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A fallback recognition override. Its code is unique within a tenant.</summary>
public sealed class FallbackRecognitionOverride(string code, string title, double amount, string region, string employeeId, string unit, int priority, int createdAt, string phone, double score, double budget, FallbackRecognitionOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public string Region { get; set; } = region;
    public string EmployeeId { get; set; } = employeeId;
    public string Unit { get; set; } = unit;
    public int Priority { get; set; } = priority;
    public int CreatedAt { get; set; } = createdAt;
    public string Phone { get; set; } = phone;
    public double Score { get; set; } = score;
    public double Budget { get; set; } = budget;
    public FallbackRecognitionOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a fallback recognition override in status planned.</summary>
    public static FallbackRecognitionOverride NewFallbackRecognitionOverride(string code, string title)
    {
        return new FallbackRecognitionOverride(
            code,
            title,
            0.0,
            "",
            "",
            "",
            0,
            0,
            "",
            0.0,
            0.0,
            FallbackRecognitionOverrideStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackRecognitionOverrideStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackRecognitionOverrideStatus next)
    {
        switch (Status)
        {
            case FallbackRecognitionOverrideStatus.Planned:
                switch (next)
                {
                    case FallbackRecognitionOverrideStatus.Scheduled:
                        return true;
                }
                return false;
            case FallbackRecognitionOverrideStatus.Scheduled:
                return next == FallbackRecognitionOverrideStatus.InProgress;
            case FallbackRecognitionOverrideStatus.InProgress:
                switch (next)
                {
                    case FallbackRecognitionOverrideStatus.Cancelled:
                        return true;
                }
                return false;
            case FallbackRecognitionOverrideStatus.Completed:
                return next == FallbackRecognitionOverrideStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(FallbackRecognitionOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback recognition override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Score + (double)CreatedAt, 2);
    }

    public FallbackRecognitionOverride Copy()
    {
        return new FallbackRecognitionOverride(
            Code,
            Title,
            Amount,
            Region,
            EmployeeId,
            Unit,
            Priority,
            CreatedAt,
            Phone,
            Score,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
