namespace Acme.Workforce.Assets.Assignment.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment rule is in its life cycle.</summary>
public enum AssignmentRuleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A assignment rule as the assignment team keeps it.</summary>
public sealed class AssignmentRule(string code, string title, double budget, double discount, int revision, string country, int attempts, string segment, AssignmentRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Budget { get; set; } = budget;
    public double Discount { get; set; } = discount;
    public int Revision { get; set; } = revision;
    public string Country { get; set; } = country;
    public int Attempts { get; set; } = attempts;
    public string Segment { get; set; } = segment;
    public AssignmentRuleStatus Status { get; set; } = status;

    /// <summary>Starts a assignment rule in status idle.</summary>
    public static AssignmentRule NewAssignmentRule(string code, string title)
    {
        return new AssignmentRule(code, title, 0.0, 0.0, 0, "", 0, "", AssignmentRuleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentRuleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentRuleStatus next)
    {
        switch (Status)
        {
            case AssignmentRuleStatus.Idle:
                switch (next)
                {
                    case AssignmentRuleStatus.Warming:
                    case AssignmentRuleStatus.Ready:
                        return true;
                }
                return false;
            case AssignmentRuleStatus.Warming:
                switch (next)
                {
                    case AssignmentRuleStatus.Ready:
                    case AssignmentRuleStatus.Stopped:
                        return true;
                }
                return false;
            case AssignmentRuleStatus.Ready:
                switch (next)
                {
                    case AssignmentRuleStatus.Draining:
                        return true;
                }
                return false;
            case AssignmentRuleStatus.Draining:
                return next == AssignmentRuleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AssignmentRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Discount - (double)Attempts, 2);
    }

    public AssignmentRule Copy()
    {
        return new AssignmentRule(Code, Title, Budget, Discount, Revision, Country, Attempts, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
