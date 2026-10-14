namespace Acme.Workforce.Engagement.PulseSurvey.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual pulse survey summary is in its life cycle.</summary>
public enum ManualPulseSurveySummaryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A manual pulse survey summary as the pulse survey team keeps it.</summary>
public sealed class ManualPulseSurveySummary(string code, string label, string unit, int updatedAt, string employeeId, double margin, string channel, double maximum, string notes, ManualPulseSurveySummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public int UpdatedAt { get; set; } = updatedAt;
    public string EmployeeId { get; set; } = employeeId;
    public double Margin { get; set; } = margin;
    public string Channel { get; set; } = channel;
    public double Maximum { get; set; } = maximum;
    public string Notes { get; set; } = notes;
    public ManualPulseSurveySummaryStatus Status { get; set; } = status;

    /// <summary>Starts a manual pulse survey summary in status draft.</summary>
    public static ManualPulseSurveySummary NewManualPulseSurveySummary(string code, string label)
    {
        return new ManualPulseSurveySummary(
            code,
            label,
            "",
            0,
            "",
            0.0,
            "",
            0.0,
            "",
            ManualPulseSurveySummaryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualPulseSurveySummaryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualPulseSurveySummaryStatus next)
    {
        switch (Status)
        {
            case ManualPulseSurveySummaryStatus.Draft:
                return next == ManualPulseSurveySummaryStatus.Closed;
            case ManualPulseSurveySummaryStatus.Pending:
                return next == ManualPulseSurveySummaryStatus.Active;
            case ManualPulseSurveySummaryStatus.Active:
                switch (next)
                {
                    case ManualPulseSurveySummaryStatus.Suspended:
                    case ManualPulseSurveySummaryStatus.Closed:
                        return true;
                }
                return false;
            case ManualPulseSurveySummaryStatus.Suspended:
                return next == ManualPulseSurveySummaryStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ManualPulseSurveySummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual pulse survey summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetValue()
    {
        return Math.Round(Margin * (double)UpdatedAt, 2);
    }

    public ManualPulseSurveySummary Copy()
    {
        return new ManualPulseSurveySummary(
            Code,
            Label,
            Unit,
            UpdatedAt,
            EmployeeId,
            Margin,
            Channel,
            Maximum,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
