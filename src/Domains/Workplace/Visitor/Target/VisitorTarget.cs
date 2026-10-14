namespace Acme.Workforce.Workplace.Visitor.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor target is in its life cycle.</summary>
public enum VisitorTargetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A visitor target as the visitor team keeps it.</summary>
public sealed class VisitorTarget(string code, string label, string badge, string category, List<string> tags, double score, VisitorTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public string Category { get; set; } = category;
    public List<string> Tags { get; set; } = tags;
    public double Score { get; set; } = score;
    public VisitorTargetStatus Status { get; set; } = status;

    /// <summary>Starts a visitor target in status draft.</summary>
    public static VisitorTarget NewVisitorTarget(string code, string label)
    {
        return new VisitorTarget(code, label, "", "", new List<string>(), 0.0, VisitorTargetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorTargetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorTargetStatus next)
    {
        switch (Status)
        {
            case VisitorTargetStatus.Draft:
                return next == VisitorTargetStatus.Closed;
            case VisitorTargetStatus.Pending:
                return next == VisitorTargetStatus.Active || next == VisitorTargetStatus.Suspended;
            case VisitorTargetStatus.Active:
                return next == VisitorTargetStatus.Suspended;
            case VisitorTargetStatus.Suspended:
                switch (next)
                {
                    case VisitorTargetStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
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

    public double MeasuredValue()
    {
        return Score;
    }

    public VisitorTarget Copy()
    {
        return new VisitorTarget(Code, Label, Badge, Category, Tags, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
