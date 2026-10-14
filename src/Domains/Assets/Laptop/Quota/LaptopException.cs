namespace Acme.Workforce.Assets.Laptop.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop exception is in its life cycle.</summary>
public enum LaptopExceptionStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A laptop exception. Its code is unique within a tenant.</summary>
public sealed class LaptopException(string code, string label, bool archived, int endsAt, string reviewer, string title, string employeeId, int tenureMonths, int batchSize, string notes, LaptopExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public int EndsAt { get; set; } = endsAt;
    public string Reviewer { get; set; } = reviewer;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public int TenureMonths { get; set; } = tenureMonths;
    public int BatchSize { get; set; } = batchSize;
    public string Notes { get; set; } = notes;
    public LaptopExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a laptop exception in status open.</summary>
    public static LaptopException NewLaptopException(string code, string label)
    {
        return new LaptopException(code, label, false, 0, "", "", "", 0, 0, "", LaptopExceptionStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopExceptionStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopExceptionStatus next)
    {
        switch (Status)
        {
            case LaptopExceptionStatus.Open:
                switch (next)
                {
                    case LaptopExceptionStatus.Archived:
                        return true;
                }
                return false;
            case LaptopExceptionStatus.InReview:
                switch (next)
                {
                    case LaptopExceptionStatus.Reopened:
                        return true;
                }
                return false;
            case LaptopExceptionStatus.Resolved:
                switch (next)
                {
                    case LaptopExceptionStatus.Reopened:
                    case LaptopExceptionStatus.Archived:
                        return true;
                }
                return false;
            case LaptopExceptionStatus.Reopened:
                return next == LaptopExceptionStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LaptopExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"laptop exception"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)EndsAt + (double)TenureMonths, 2);
    }

    public LaptopException Copy()
    {
        return new LaptopException(
            Code,
            Label,
            Archived,
            EndsAt,
            Reviewer,
            Title,
            EmployeeId,
            TenureMonths,
            BatchSize,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
