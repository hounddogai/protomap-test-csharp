namespace Acme.Workforce.Compensation.MeritCycle.CycleOverride;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual merit cycle override is in its life cycle.</summary>
public enum ManualMeritCycleOverrideStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a manual merit cycle override and its current status.</summary>
public sealed class ManualMeritCycleOverride(string code, string label, string reviewer, int graceDays, double margin, int sequence, string managerId, double threshold, string ownerId, double weight, ManualMeritCycleOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public int GraceDays { get; set; } = graceDays;
    public double Margin { get; set; } = margin;
    public int Sequence { get; set; } = sequence;
    public string ManagerId { get; set; } = managerId;
    public double Threshold { get; set; } = threshold;
    public string OwnerId { get; set; } = ownerId;
    public double Weight { get; set; } = weight;
    public ManualMeritCycleOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a manual merit cycle override in status draft.</summary>
    public static ManualMeritCycleOverride NewManualMeritCycleOverride(string code, string label)
    {
        return new ManualMeritCycleOverride(
            code,
            label,
            "",
            0,
            0.0,
            0,
            "",
            0.0,
            "",
            0.0,
            ManualMeritCycleOverrideStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualMeritCycleOverrideStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualMeritCycleOverrideStatus next)
    {
        switch (Status)
        {
            case ManualMeritCycleOverrideStatus.Draft:
                return next == ManualMeritCycleOverrideStatus.Active || next == ManualMeritCycleOverrideStatus.Closed;
            case ManualMeritCycleOverrideStatus.Pending:
                return next == ManualMeritCycleOverrideStatus.Active;
            case ManualMeritCycleOverrideStatus.Active:
                switch (next)
                {
                    case ManualMeritCycleOverrideStatus.Suspended:
                        return true;
                }
                return false;
            case ManualMeritCycleOverrideStatus.Suspended:
                return next == ManualMeritCycleOverrideStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ManualMeritCycleOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual merit cycle override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)GraceDays - (double)Sequence, 2);
    }

    public ManualMeritCycleOverride Copy()
    {
        return new ManualMeritCycleOverride(
            Code,
            Label,
            Reviewer,
            GraceDays,
            Margin,
            Sequence,
            ManagerId,
            Threshold,
            OwnerId,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
