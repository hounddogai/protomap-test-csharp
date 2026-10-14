namespace Acme.Workforce.Assets.Assignment.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment rollup is in its life cycle.</summary>
public enum AssignmentRollupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a assignment rollup and its current status.</summary>
public sealed class AssignmentRollup(string code, string title, string reviewer, double rate, double amount, double minimum, double threshold, Dictionary<string, string> attributes, AssignmentRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public double Rate { get; set; } = rate;
    public double Amount { get; set; } = amount;
    public double Minimum { get; set; } = minimum;
    public double Threshold { get; set; } = threshold;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public AssignmentRollupStatus Status { get; set; } = status;

    /// <summary>Starts a assignment rollup in status new.</summary>
    public static AssignmentRollup NewAssignmentRollup(string code, string title)
    {
        return new AssignmentRollup(
            code,
            title,
            "",
            0.0,
            0.0,
            0.0,
            0.0,
            new Dictionary<string, string>(),
            AssignmentRollupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentRollupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentRollupStatus next)
    {
        switch (Status)
        {
            case AssignmentRollupStatus.New:
                return next == AssignmentRollupStatus.Done || next == AssignmentRollupStatus.Failed;
            case AssignmentRollupStatus.Queued:
                return next == AssignmentRollupStatus.Done;
            case AssignmentRollupStatus.Running:
                switch (next)
                {
                    case AssignmentRollupStatus.Failed:
                        return true;
                }
                return false;
            case AssignmentRollupStatus.Done:
                switch (next)
                {
                    case AssignmentRollupStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssignmentRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
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

    public double ProjectedLoad()
    {
        return Math.Round(Amount - Minimum, 2);
    }

    public AssignmentRollup Copy()
    {
        return new AssignmentRollup(Code, Title, Reviewer, Rate, Amount, Minimum, Threshold, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
