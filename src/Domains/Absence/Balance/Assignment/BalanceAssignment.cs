namespace Acme.Workforce.Absence.Balance.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance assignment is in its life cycle.</summary>
public enum BalanceAssignmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One balance assignment, with what the planners need to act on it.</summary>
public sealed class BalanceAssignment(string code, string title, double rate, string source, bool active, double fte, double ceiling, Dictionary<string, string> attributes, double minimum, string grade, int graceDays, BalanceAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public string Source { get; set; } = source;
    public bool Active { get; set; } = active;
    public double Fte { get; set; } = fte;
    public double Ceiling { get; set; } = ceiling;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Minimum { get; set; } = minimum;
    public string Grade { get; set; } = grade;
    public int GraceDays { get; set; } = graceDays;
    public BalanceAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a balance assignment in status new.</summary>
    public static BalanceAssignment NewBalanceAssignment(string code, string title)
    {
        return new BalanceAssignment(
            code,
            title,
            0.0,
            "",
            false,
            0.0,
            0.0,
            new Dictionary<string, string>(),
            0.0,
            "",
            0,
            BalanceAssignmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceAssignmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceAssignmentStatus next)
    {
        switch (Status)
        {
            case BalanceAssignmentStatus.New:
                return next == BalanceAssignmentStatus.Running || next == BalanceAssignmentStatus.Failed;
            case BalanceAssignmentStatus.Queued:
                switch (next)
                {
                    case BalanceAssignmentStatus.Done:
                        return true;
                }
                return false;
            case BalanceAssignmentStatus.Running:
                return next == BalanceAssignmentStatus.Done;
            case BalanceAssignmentStatus.Done:
                return next == BalanceAssignmentStatus.Failed;
        }
        return false;
    }

    public void MoveTo(BalanceAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"balance assignment"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double EffectiveExposure()
    {
        return Math.Round(Rate - Minimum, 2);
    }

    public BalanceAssignment Copy()
    {
        return new BalanceAssignment(
            Code,
            Title,
            Rate,
            Source,
            Active,
            Fte,
            Ceiling,
            Attributes,
            Minimum,
            Grade,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
