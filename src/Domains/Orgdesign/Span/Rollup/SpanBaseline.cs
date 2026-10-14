namespace Acme.Workforce.Orgdesign.Span.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span baseline is in its life cycle.</summary>
public enum SpanBaselineStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a span baseline and its current status.</summary>
public sealed class SpanBaseline(string code, string name, double threshold, double hours, double maximum, double floorValue, string ownerId, string currency, int sequence, string employeeId, int leaveDays, SpanBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public double Hours { get; set; } = hours;
    public double Maximum { get; set; } = maximum;
    public double FloorValue { get; set; } = floorValue;
    public string OwnerId { get; set; } = ownerId;
    public string Currency { get; set; } = currency;
    public int Sequence { get; set; } = sequence;
    public string EmployeeId { get; set; } = employeeId;
    public int LeaveDays { get; set; } = leaveDays;
    public SpanBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a span baseline in status created.</summary>
    public static SpanBaseline NewSpanBaseline(string code, string name)
    {
        return new SpanBaseline(code, name, 0.0, 0.0, 0.0, 0.0, "", "", 0, "", 0, SpanBaselineStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanBaselineStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanBaselineStatus next)
    {
        switch (Status)
        {
            case SpanBaselineStatus.Created:
                return next == SpanBaselineStatus.Validated || next == SpanBaselineStatus.Retired;
            case SpanBaselineStatus.Validated:
                return next == SpanBaselineStatus.Published || next == SpanBaselineStatus.Retired;
            case SpanBaselineStatus.Published:
                return next == SpanBaselineStatus.Retired;
        }
        return false;
    }

    public void MoveTo(SpanBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(FloorValue * (double)Sequence, 2);
    }

    public SpanBaseline Copy()
    {
        return new SpanBaseline(
            Code,
            Name,
            Threshold,
            Hours,
            Maximum,
            FloorValue,
            OwnerId,
            Currency,
            Sequence,
            EmployeeId,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
