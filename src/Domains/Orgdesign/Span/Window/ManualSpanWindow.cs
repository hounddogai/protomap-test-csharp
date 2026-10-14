namespace Acme.Workforce.Orgdesign.Span.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual span window is in its life cycle.</summary>
public enum ManualSpanWindowStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A manual span window. Its code is unique within a tenant.</summary>
public sealed class ManualSpanWindow(string code, string title, string employeeId, int graceDays, bool approved, int revision, double ratio, int capacity, int durationMinutes, ManualSpanWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public int GraceDays { get; set; } = graceDays;
    public bool Approved { get; set; } = approved;
    public int Revision { get; set; } = revision;
    public double Ratio { get; set; } = ratio;
    public int Capacity { get; set; } = capacity;
    public int DurationMinutes { get; set; } = durationMinutes;
    public ManualSpanWindowStatus Status { get; set; } = status;

    /// <summary>Starts a manual span window in status draft.</summary>
    public static ManualSpanWindow NewManualSpanWindow(string code, string title)
    {
        return new ManualSpanWindow(code, title, "", 0, false, 0, 0.0, 0, 0, ManualSpanWindowStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualSpanWindowStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualSpanWindowStatus next)
    {
        switch (Status)
        {
            case ManualSpanWindowStatus.Draft:
                switch (next)
                {
                    case ManualSpanWindowStatus.Pending:
                    case ManualSpanWindowStatus.Closed:
                        return true;
                }
                return false;
            case ManualSpanWindowStatus.Pending:
                switch (next)
                {
                    case ManualSpanWindowStatus.Active:
                    case ManualSpanWindowStatus.Closed:
                        return true;
                }
                return false;
            case ManualSpanWindowStatus.Active:
                return next == ManualSpanWindowStatus.Closed;
            case ManualSpanWindowStatus.Suspended:
                switch (next)
                {
                    case ManualSpanWindowStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualSpanWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual span window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Ratio - (double)DurationMinutes, 2);
    }

    public ManualSpanWindow Copy()
    {
        return new ManualSpanWindow(
            Code,
            Title,
            EmployeeId,
            GraceDays,
            Approved,
            Revision,
            Ratio,
            Capacity,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
