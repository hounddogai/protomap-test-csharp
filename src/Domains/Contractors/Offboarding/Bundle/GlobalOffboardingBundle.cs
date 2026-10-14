namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global offboarding bundle is in its life cycle.</summary>
public enum GlobalOffboardingBundleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a global offboarding bundle and its current status.</summary>
public sealed class GlobalOffboardingBundle(string code, string name, bool active, double budget, string summary, string contactEmail, GlobalOffboardingBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public double Budget { get; set; } = budget;
    public string Summary { get; set; } = summary;
    public string ContactEmail { get; set; } = contactEmail;
    public GlobalOffboardingBundleStatus Status { get; set; } = status;

    /// <summary>Starts a global offboarding bundle in status proposed.</summary>
    public static GlobalOffboardingBundle NewGlobalOffboardingBundle(string code, string name)
    {
        return new GlobalOffboardingBundle(code, name, false, 0.0, "", "", GlobalOffboardingBundleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalOffboardingBundleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalOffboardingBundleStatus next)
    {
        switch (Status)
        {
            case GlobalOffboardingBundleStatus.Proposed:
                return next == GlobalOffboardingBundleStatus.Approved || next == GlobalOffboardingBundleStatus.Rejected;
            case GlobalOffboardingBundleStatus.Approved:
                return next == GlobalOffboardingBundleStatus.Rejected || next == GlobalOffboardingBundleStatus.Withdrawn;
            case GlobalOffboardingBundleStatus.Rejected:
                return next == GlobalOffboardingBundleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(GlobalOffboardingBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global offboarding bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public GlobalOffboardingBundle Copy()
    {
        return new GlobalOffboardingBundle(Code, Name, Active, Budget, Summary, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
