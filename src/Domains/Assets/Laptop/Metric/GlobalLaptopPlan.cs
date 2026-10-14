namespace Acme.Workforce.Assets.Laptop.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global laptop plan is in its life cycle.</summary>
public enum GlobalLaptopPlanStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A global laptop plan as the laptop team keeps it.</summary>
public sealed class GlobalLaptopPlan(string code, string label, string segment, string channel, string contactEmail, string skill, string grade, double amount, GlobalLaptopPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public string Channel { get; set; } = channel;
    public string ContactEmail { get; set; } = contactEmail;
    public string Skill { get; set; } = skill;
    public string Grade { get; set; } = grade;
    public double Amount { get; set; } = amount;
    public GlobalLaptopPlanStatus Status { get; set; } = status;

    /// <summary>Starts a global laptop plan in status open.</summary>
    public static GlobalLaptopPlan NewGlobalLaptopPlan(string code, string label)
    {
        return new GlobalLaptopPlan(code, label, "", "", "", "", "", 0.0, GlobalLaptopPlanStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalLaptopPlanStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalLaptopPlanStatus next)
    {
        switch (Status)
        {
            case GlobalLaptopPlanStatus.Open:
                return next == GlobalLaptopPlanStatus.InReview || next == GlobalLaptopPlanStatus.Reopened;
            case GlobalLaptopPlanStatus.InReview:
                return next == GlobalLaptopPlanStatus.Archived;
            case GlobalLaptopPlanStatus.Resolved:
                switch (next)
                {
                    case GlobalLaptopPlanStatus.Reopened:
                    case GlobalLaptopPlanStatus.Archived:
                        return true;
                }
                return false;
            case GlobalLaptopPlanStatus.Reopened:
                switch (next)
                {
                    case GlobalLaptopPlanStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalLaptopPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global laptop plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public GlobalLaptopPlan Copy()
    {
        return new GlobalLaptopPlan(Code, Label, Segment, Channel, ContactEmail, Skill, Grade, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
