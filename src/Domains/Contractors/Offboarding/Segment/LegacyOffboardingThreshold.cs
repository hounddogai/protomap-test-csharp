namespace Acme.Workforce.Contractors.Offboarding.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy offboarding threshold is in its life cycle.</summary>
public enum LegacyOffboardingThresholdStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A legacy offboarding threshold. Its code is unique within a tenant.</summary>
public sealed class LegacyOffboardingThreshold(string code, string title, double margin, bool visible, double ratio, LegacyOffboardingThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public bool Visible { get; set; } = visible;
    public double Ratio { get; set; } = ratio;
    public LegacyOffboardingThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a legacy offboarding threshold in status proposed.</summary>
    public static LegacyOffboardingThreshold NewLegacyOffboardingThreshold(string code, string title)
    {
        return new LegacyOffboardingThreshold(
            code,
            title,
            0.0,
            false,
            0.0,
            LegacyOffboardingThresholdStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyOffboardingThresholdStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyOffboardingThresholdStatus next)
    {
        switch (Status)
        {
            case LegacyOffboardingThresholdStatus.Proposed:
                return next == LegacyOffboardingThresholdStatus.Approved || next == LegacyOffboardingThresholdStatus.Rejected;
            case LegacyOffboardingThresholdStatus.Approved:
                return next == LegacyOffboardingThresholdStatus.Rejected || next == LegacyOffboardingThresholdStatus.Withdrawn;
            case LegacyOffboardingThresholdStatus.Rejected:
                switch (next)
                {
                    case LegacyOffboardingThresholdStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyOffboardingThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy offboarding threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Margin + Ratio, 2);
    }

    public LegacyOffboardingThreshold Copy()
    {
        return new LegacyOffboardingThreshold(Code, Title, Margin, Visible, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
