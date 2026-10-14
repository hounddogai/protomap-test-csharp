namespace Acme.Workforce.Contractors.Offboarding.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding adjustment is in its life cycle.</summary>
public enum OffboardingAdjustmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A offboarding adjustment. Its code is unique within a tenant.</summary>
public sealed class OffboardingAdjustment(string code, string name, double rating, string department, string externalRef, bool visible, double maximum, double rate, List<string> tags, OffboardingAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rating { get; set; } = rating;
    public string Department { get; set; } = department;
    public string ExternalRef { get; set; } = externalRef;
    public bool Visible { get; set; } = visible;
    public double Maximum { get; set; } = maximum;
    public double Rate { get; set; } = rate;
    public List<string> Tags { get; set; } = tags;
    public OffboardingAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding adjustment in status planned.</summary>
    public static OffboardingAdjustment NewOffboardingAdjustment(string code, string name)
    {
        return new OffboardingAdjustment(
            code,
            name,
            0.0,
            "",
            "",
            false,
            0.0,
            0.0,
            new List<string>(),
            OffboardingAdjustmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingAdjustmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingAdjustmentStatus next)
    {
        switch (Status)
        {
            case OffboardingAdjustmentStatus.Planned:
                switch (next)
                {
                    case OffboardingAdjustmentStatus.Scheduled:
                    case OffboardingAdjustmentStatus.Completed:
                        return true;
                }
                return false;
            case OffboardingAdjustmentStatus.Scheduled:
                return next == OffboardingAdjustmentStatus.Cancelled;
            case OffboardingAdjustmentStatus.InProgress:
                return next == OffboardingAdjustmentStatus.Completed || next == OffboardingAdjustmentStatus.Cancelled;
            case OffboardingAdjustmentStatus.Completed:
                return next == OffboardingAdjustmentStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(OffboardingAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"offboarding adjustment"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedValue()
    {
        return Math.Round(Maximum - Rate, 2);
    }

    public OffboardingAdjustment Copy()
    {
        return new OffboardingAdjustment(
            Code,
            Name,
            Rating,
            Department,
            ExternalRef,
            Visible,
            Maximum,
            Rate,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
