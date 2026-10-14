namespace Acme.Workforce.Workplace.Desk.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk entry is in its life cycle.</summary>
public enum DeskEntryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A desk entry. Its code is unique within a tenant.</summary>
public sealed class DeskEntry(string code, string label, int windowDays, string employeeId, string department, int endsAt, int version, string externalRef, int tenureMonths, string locale, bool active, DeskEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public string EmployeeId { get; set; } = employeeId;
    public string Department { get; set; } = department;
    public int EndsAt { get; set; } = endsAt;
    public int Version { get; set; } = version;
    public string ExternalRef { get; set; } = externalRef;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Locale { get; set; } = locale;
    public bool Active { get; set; } = active;
    public DeskEntryStatus Status { get; set; } = status;

    /// <summary>Starts a desk entry in status idle.</summary>
    public static DeskEntry NewDeskEntry(string code, string label)
    {
        return new DeskEntry(code, label, 0, "", "", 0, 0, "", 0, "", false, DeskEntryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskEntryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskEntryStatus next)
    {
        switch (Status)
        {
            case DeskEntryStatus.Idle:
                return next == DeskEntryStatus.Warming || next == DeskEntryStatus.Stopped;
            case DeskEntryStatus.Warming:
                switch (next)
                {
                    case DeskEntryStatus.Ready:
                        return true;
                }
                return false;
            case DeskEntryStatus.Ready:
                switch (next)
                {
                    case DeskEntryStatus.Draining:
                        return true;
                }
                return false;
            case DeskEntryStatus.Draining:
                switch (next)
                {
                    case DeskEntryStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeskEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)EndsAt + (double)Version, 2);
    }

    public DeskEntry Copy()
    {
        return new DeskEntry(
            Code,
            Label,
            WindowDays,
            EmployeeId,
            Department,
            EndsAt,
            Version,
            ExternalRef,
            TenureMonths,
            Locale,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
