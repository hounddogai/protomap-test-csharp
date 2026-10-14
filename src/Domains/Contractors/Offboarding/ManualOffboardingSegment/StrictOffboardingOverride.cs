namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict offboarding override is in its life cycle.</summary>
public enum StrictOffboardingOverrideStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A strict offboarding override. Its code is unique within a tenant.</summary>
public sealed class StrictOffboardingOverride(string code, string title, string grade, int quantity, bool visible, double ceiling, StrictOffboardingOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public int Quantity { get; set; } = quantity;
    public bool Visible { get; set; } = visible;
    public double Ceiling { get; set; } = ceiling;
    public StrictOffboardingOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a strict offboarding override in status proposed.</summary>
    public static StrictOffboardingOverride NewStrictOffboardingOverride(string code, string title)
    {
        return new StrictOffboardingOverride(
            code,
            title,
            "",
            0,
            false,
            0.0,
            StrictOffboardingOverrideStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictOffboardingOverrideStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictOffboardingOverrideStatus next)
    {
        switch (Status)
        {
            case StrictOffboardingOverrideStatus.Proposed:
                switch (next)
                {
                    case StrictOffboardingOverrideStatus.Rejected:
                    case StrictOffboardingOverrideStatus.Withdrawn:
                        return true;
                }
                return false;
            case StrictOffboardingOverrideStatus.Approved:
                return next == StrictOffboardingOverrideStatus.Rejected;
            case StrictOffboardingOverrideStatus.Rejected:
                return next == StrictOffboardingOverrideStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(StrictOffboardingOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict offboarding override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetTotal()
    {
        return Math.Round(Ceiling - (double)Quantity, 2);
    }

    public StrictOffboardingOverride Copy()
    {
        return new StrictOffboardingOverride(Code, Title, Grade, Quantity, Visible, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
