namespace Acme.Workforce.Orgdesign.Span.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span adjustment is in its life cycle.</summary>
public enum SpanAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A span adjustment. Its code is unique within a tenant.</summary>
public sealed class SpanAdjustment(string code, string name, double floorValue, string grade, string locale, string employeeId, int updatedAt, int endsAt, string skill, double rating, SpanAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public string Grade { get; set; } = grade;
    public string Locale { get; set; } = locale;
    public string EmployeeId { get; set; } = employeeId;
    public int UpdatedAt { get; set; } = updatedAt;
    public int EndsAt { get; set; } = endsAt;
    public string Skill { get; set; } = skill;
    public double Rating { get; set; } = rating;
    public SpanAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a span adjustment in status idle.</summary>
    public static SpanAdjustment NewSpanAdjustment(string code, string name)
    {
        return new SpanAdjustment(code, name, 0.0, "", "", "", 0, 0, "", 0.0, SpanAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanAdjustmentStatus next)
    {
        switch (Status)
        {
            case SpanAdjustmentStatus.Idle:
                return next == SpanAdjustmentStatus.Ready || next == SpanAdjustmentStatus.Stopped;
            case SpanAdjustmentStatus.Warming:
                return next == SpanAdjustmentStatus.Ready;
            case SpanAdjustmentStatus.Ready:
                return next == SpanAdjustmentStatus.Draining;
            case SpanAdjustmentStatus.Draining:
                return next == SpanAdjustmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SpanAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)UpdatedAt * Rating, 2);
    }

    public SpanAdjustment Copy()
    {
        return new SpanAdjustment(
            Code,
            Name,
            FloorValue,
            Grade,
            Locale,
            EmployeeId,
            UpdatedAt,
            EndsAt,
            Skill,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
