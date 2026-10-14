namespace Acme.Workforce.Assets.Assignment.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment threshold is in its life cycle.</summary>
public enum AssignmentThresholdStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a assignment threshold and its current status.</summary>
public sealed class AssignmentThreshold(string code, string label, double margin, double rate, double budget, double minimum, AssignmentThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public double Rate { get; set; } = rate;
    public double Budget { get; set; } = budget;
    public double Minimum { get; set; } = minimum;
    public AssignmentThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a assignment threshold in status proposed.</summary>
    public static AssignmentThreshold NewAssignmentThreshold(string code, string label)
    {
        return new AssignmentThreshold(code, label, 0.0, 0.0, 0.0, 0.0, AssignmentThresholdStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentThresholdStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentThresholdStatus next)
    {
        switch (Status)
        {
            case AssignmentThresholdStatus.Proposed:
                return next == AssignmentThresholdStatus.Rejected;
            case AssignmentThresholdStatus.Approved:
                switch (next)
                {
                    case AssignmentThresholdStatus.Withdrawn:
                        return true;
                }
                return false;
            case AssignmentThresholdStatus.Rejected:
                switch (next)
                {
                    case AssignmentThresholdStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssignmentThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetValue()
    {
        return Math.Round(Minimum * Rate, 2);
    }

    public AssignmentThreshold Copy()
    {
        return new AssignmentThreshold(Code, Label, Margin, Rate, Budget, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
