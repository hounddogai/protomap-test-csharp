namespace Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat assignment is in its life cycle.</summary>
public enum LicenseSeatAssignmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A license seat assignment as the license seat team keeps it.</summary>
public sealed class LicenseSeatAssignment(string code, string label, double margin, double minimum, int updatedAt, string grade, string locale, string source, LicenseSeatAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public double Minimum { get; set; } = minimum;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Grade { get; set; } = grade;
    public string Locale { get; set; } = locale;
    public string Source { get; set; } = source;
    public LicenseSeatAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a license seat assignment in status idle.</summary>
    public static LicenseSeatAssignment NewLicenseSeatAssignment(string code, string label)
    {
        return new LicenseSeatAssignment(code, label, 0.0, 0.0, 0, "", "", "", LicenseSeatAssignmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatAssignmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatAssignmentStatus next)
    {
        switch (Status)
        {
            case LicenseSeatAssignmentStatus.Idle:
                return next == LicenseSeatAssignmentStatus.Draining;
            case LicenseSeatAssignmentStatus.Warming:
                return next == LicenseSeatAssignmentStatus.Ready;
            case LicenseSeatAssignmentStatus.Ready:
                return next == LicenseSeatAssignmentStatus.Draining || next == LicenseSeatAssignmentStatus.Stopped;
            case LicenseSeatAssignmentStatus.Draining:
                return next == LicenseSeatAssignmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LicenseSeatAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Minimum * (double)UpdatedAt, 2);
    }

    public LicenseSeatAssignment Copy()
    {
        return new LicenseSeatAssignment(Code, Label, Margin, Minimum, UpdatedAt, Grade, Locale, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
