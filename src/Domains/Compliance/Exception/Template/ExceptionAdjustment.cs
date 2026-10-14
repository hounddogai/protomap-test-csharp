namespace Acme.Workforce.Compliance.Exception.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception adjustment is in its life cycle.</summary>
public enum ExceptionAdjustmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A exception adjustment. Its code is unique within a tenant.</summary>
public sealed class ExceptionAdjustment(string code, string title, string location, double margin, int expiresAt, string notes, int attempts, string region, string badge, double hours, double threshold, ExceptionAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public double Margin { get; set; } = margin;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Notes { get; set; } = notes;
    public int Attempts { get; set; } = attempts;
    public string Region { get; set; } = region;
    public string Badge { get; set; } = badge;
    public double Hours { get; set; } = hours;
    public double Threshold { get; set; } = threshold;
    public ExceptionAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a exception adjustment in status draft.</summary>
    public static ExceptionAdjustment NewExceptionAdjustment(string code, string title)
    {
        return new ExceptionAdjustment(
            code,
            title,
            "",
            0.0,
            0,
            "",
            0,
            "",
            "",
            0.0,
            0.0,
            ExceptionAdjustmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionAdjustmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionAdjustmentStatus next)
    {
        switch (Status)
        {
            case ExceptionAdjustmentStatus.Draft:
                return next == ExceptionAdjustmentStatus.Suspended;
            case ExceptionAdjustmentStatus.Pending:
                switch (next)
                {
                    case ExceptionAdjustmentStatus.Active:
                    case ExceptionAdjustmentStatus.Closed:
                        return true;
                }
                return false;
            case ExceptionAdjustmentStatus.Active:
                return next == ExceptionAdjustmentStatus.Closed;
            case ExceptionAdjustmentStatus.Suspended:
                switch (next)
                {
                    case ExceptionAdjustmentStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ExceptionAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"exception adjustment"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)ExpiresAt + Threshold, 2);
    }

    public ExceptionAdjustment Copy()
    {
        return new ExceptionAdjustment(
            Code,
            Title,
            Location,
            Margin,
            ExpiresAt,
            Notes,
            Attempts,
            Region,
            Badge,
            Hours,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
