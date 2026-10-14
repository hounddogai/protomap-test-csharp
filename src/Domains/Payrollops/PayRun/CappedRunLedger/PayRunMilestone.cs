namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run milestone is in its life cycle.</summary>
public enum PayRunMilestoneStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A pay run milestone as the pay run team keeps it.</summary>
public sealed class PayRunMilestone(string code, string label, double ceiling, double discount, double rating, string name, PayRunMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public double Discount { get; set; } = discount;
    public double Rating { get; set; } = rating;
    public string Name { get; set; } = name;
    public PayRunMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a pay run milestone in status created.</summary>
    public static PayRunMilestone NewPayRunMilestone(string code, string label)
    {
        return new PayRunMilestone(code, label, 0.0, 0.0, 0.0, "", PayRunMilestoneStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunMilestoneStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunMilestoneStatus next)
    {
        switch (Status)
        {
            case PayRunMilestoneStatus.Created:
                switch (next)
                {
                    case PayRunMilestoneStatus.Validated:
                        return true;
                }
                return false;
            case PayRunMilestoneStatus.Validated:
                return next == PayRunMilestoneStatus.Published || next == PayRunMilestoneStatus.Retired;
            case PayRunMilestoneStatus.Published:
                return next == PayRunMilestoneStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PayRunMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pay run milestone"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Discount * Ceiling, 2);
    }

    public PayRunMilestone Copy()
    {
        return new PayRunMilestone(Code, Label, Ceiling, Discount, Rating, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
