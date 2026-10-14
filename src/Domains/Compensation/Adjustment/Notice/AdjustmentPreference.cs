namespace Acme.Workforce.Compensation.Adjustment.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment preference is in its life cycle.</summary>
public enum AdjustmentPreferenceStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A adjustment preference. Its code is unique within a tenant.</summary>
public sealed class AdjustmentPreference(string code, string label, double minimum, string currency, double budget, string title, AdjustmentPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public string Currency { get; set; } = currency;
    public double Budget { get; set; } = budget;
    public string Title { get; set; } = title;
    public AdjustmentPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment preference in status proposed.</summary>
    public static AdjustmentPreference NewAdjustmentPreference(string code, string label)
    {
        return new AdjustmentPreference(code, label, 0.0, "", 0.0, "", AdjustmentPreferenceStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentPreferenceStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentPreferenceStatus next)
    {
        switch (Status)
        {
            case AdjustmentPreferenceStatus.Proposed:
                return next == AdjustmentPreferenceStatus.Rejected;
            case AdjustmentPreferenceStatus.Approved:
                return next == AdjustmentPreferenceStatus.Rejected || next == AdjustmentPreferenceStatus.Withdrawn;
            case AdjustmentPreferenceStatus.Rejected:
                switch (next)
                {
                    case AdjustmentPreferenceStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdjustmentPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetExposure()
    {
        return Math.Round(Budget - Minimum, 2);
    }

    public AdjustmentPreference Copy()
    {
        return new AdjustmentPreference(Code, Label, Minimum, Currency, Budget, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
