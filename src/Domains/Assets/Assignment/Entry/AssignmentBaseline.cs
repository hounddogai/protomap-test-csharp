namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment baseline is in its life cycle.</summary>
public enum AssignmentBaselineStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a assignment baseline and its current status.</summary>
public sealed class AssignmentBaseline(string code, string label, double hours, double weight, double amount, string checksum, bool visible, double ratio, bool pinned, string contactEmail, AssignmentBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public double Weight { get; set; } = weight;
    public double Amount { get; set; } = amount;
    public string Checksum { get; set; } = checksum;
    public bool Visible { get; set; } = visible;
    public double Ratio { get; set; } = ratio;
    public bool Pinned { get; set; } = pinned;
    public string ContactEmail { get; set; } = contactEmail;
    public AssignmentBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a assignment baseline in status open.</summary>
    public static AssignmentBaseline NewAssignmentBaseline(string code, string label)
    {
        return new AssignmentBaseline(
            code,
            label,
            0.0,
            0.0,
            0.0,
            "",
            false,
            0.0,
            false,
            "",
            AssignmentBaselineStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentBaselineStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentBaselineStatus next)
    {
        switch (Status)
        {
            case AssignmentBaselineStatus.Open:
                switch (next)
                {
                    case AssignmentBaselineStatus.Resolved:
                    case AssignmentBaselineStatus.Archived:
                        return true;
                }
                return false;
            case AssignmentBaselineStatus.InReview:
                return next == AssignmentBaselineStatus.Resolved || next == AssignmentBaselineStatus.Reopened;
            case AssignmentBaselineStatus.Resolved:
                switch (next)
                {
                    case AssignmentBaselineStatus.Reopened:
                        return true;
                }
                return false;
            case AssignmentBaselineStatus.Reopened:
                switch (next)
                {
                    case AssignmentBaselineStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssignmentBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Weight * Amount, 2);
    }

    public AssignmentBaseline Copy()
    {
        return new AssignmentBaseline(
            Code,
            Label,
            Hours,
            Weight,
            Amount,
            Checksum,
            Visible,
            Ratio,
            Pinned,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
