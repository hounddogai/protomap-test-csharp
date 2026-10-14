namespace Acme.Workforce.Compliance.Exception.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception target is in its life cycle.</summary>
public enum ExceptionTargetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A exception target as the exception team keeps it.</summary>
public sealed class ExceptionTarget(string code, string label, int batchSize, int updatedAt, string channel, string description, double maximum, double floorValue, ExceptionTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Channel { get; set; } = channel;
    public string Description { get; set; } = description;
    public double Maximum { get; set; } = maximum;
    public double FloorValue { get; set; } = floorValue;
    public ExceptionTargetStatus Status { get; set; } = status;

    /// <summary>Starts a exception target in status draft.</summary>
    public static ExceptionTarget NewExceptionTarget(string code, string label)
    {
        return new ExceptionTarget(code, label, 0, 0, "", "", 0.0, 0.0, ExceptionTargetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionTargetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionTargetStatus next)
    {
        switch (Status)
        {
            case ExceptionTargetStatus.Draft:
                return next == ExceptionTargetStatus.Closed;
            case ExceptionTargetStatus.Pending:
                switch (next)
                {
                    case ExceptionTargetStatus.Suspended:
                    case ExceptionTargetStatus.Closed:
                        return true;
                }
                return false;
            case ExceptionTargetStatus.Active:
                switch (next)
                {
                    case ExceptionTargetStatus.Suspended:
                    case ExceptionTargetStatus.Closed:
                        return true;
                }
                return false;
            case ExceptionTargetStatus.Suspended:
                switch (next)
                {
                    case ExceptionTargetStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ExceptionTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Maximum + (double)BatchSize, 2);
    }

    public ExceptionTarget Copy()
    {
        return new ExceptionTarget(
            Code,
            Label,
            BatchSize,
            UpdatedAt,
            Channel,
            Description,
            Maximum,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
