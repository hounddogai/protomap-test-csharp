namespace Acme.Workforce.Workplace.Visitor.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor rule is in its life cycle.</summary>
public enum VisitorRuleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A visitor rule as the visitor team keeps it.</summary>
public sealed class VisitorRule(string code, string title, bool approved, double score, int batchSize, double maximum, double threshold, double floorValue, string team, VisitorRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public double Score { get; set; } = score;
    public int BatchSize { get; set; } = batchSize;
    public double Maximum { get; set; } = maximum;
    public double Threshold { get; set; } = threshold;
    public double FloorValue { get; set; } = floorValue;
    public string Team { get; set; } = team;
    public VisitorRuleStatus Status { get; set; } = status;

    /// <summary>Starts a visitor rule in status idle.</summary>
    public static VisitorRule NewVisitorRule(string code, string title)
    {
        return new VisitorRule(code, title, false, 0.0, 0, 0.0, 0.0, 0.0, "", VisitorRuleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorRuleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorRuleStatus next)
    {
        switch (Status)
        {
            case VisitorRuleStatus.Idle:
                switch (next)
                {
                    case VisitorRuleStatus.Ready:
                        return true;
                }
                return false;
            case VisitorRuleStatus.Warming:
                switch (next)
                {
                    case VisitorRuleStatus.Draining:
                        return true;
                }
                return false;
            case VisitorRuleStatus.Ready:
                switch (next)
                {
                    case VisitorRuleStatus.Draining:
                    case VisitorRuleStatus.Stopped:
                        return true;
                }
                return false;
            case VisitorRuleStatus.Draining:
                switch (next)
                {
                    case VisitorRuleStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Threshold * Score, 2);
    }

    public VisitorRule Copy()
    {
        return new VisitorRule(
            Code,
            Title,
            Approved,
            Score,
            BatchSize,
            Maximum,
            Threshold,
            FloorValue,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
