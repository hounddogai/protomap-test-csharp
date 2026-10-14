namespace Acme.Workforce.Contractors.Offboarding.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding override is in its life cycle.</summary>
public enum OffboardingOverrideStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a offboarding override and its current status.</summary>
public sealed class OffboardingOverride(string code, string label, string managerId, int priority, string phone, double ceiling, string category, OffboardingOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ManagerId { get; set; } = managerId;
    public int Priority { get; set; } = priority;
    public string Phone { get; set; } = phone;
    public double Ceiling { get; set; } = ceiling;
    public string Category { get; set; } = category;
    public OffboardingOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding override in status created.</summary>
    public static OffboardingOverride NewOffboardingOverride(string code, string label)
    {
        return new OffboardingOverride(code, label, "", 0, "", 0.0, "", OffboardingOverrideStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingOverrideStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingOverrideStatus next)
    {
        switch (Status)
        {
            case OffboardingOverrideStatus.Created:
                return next == OffboardingOverrideStatus.Validated;
            case OffboardingOverrideStatus.Validated:
                return next == OffboardingOverrideStatus.Published;
            case OffboardingOverrideStatus.Published:
                switch (next)
                {
                    case OffboardingOverrideStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OffboardingOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Ceiling - (double)Priority, 2);
    }

    public OffboardingOverride Copy()
    {
        return new OffboardingOverride(Code, Label, ManagerId, Priority, Phone, Ceiling, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
