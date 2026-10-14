namespace Acme.Workforce.Contractors.StatementOfWork.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work milestone is in its life cycle.</summary>
public enum StatementOfWorkMilestoneStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A statement of work milestone. Its code is unique within a tenant.</summary>
public sealed class StatementOfWorkMilestone(string code, string name, string employeeId, int windowDays, string ownerId, double minimum, Dictionary<string, string> attributes, int graceDays, StatementOfWorkMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public int WindowDays { get; set; } = windowDays;
    public string OwnerId { get; set; } = ownerId;
    public double Minimum { get; set; } = minimum;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int GraceDays { get; set; } = graceDays;
    public StatementOfWorkMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work milestone in status new.</summary>
    public static StatementOfWorkMilestone NewStatementOfWorkMilestone(string code, string name)
    {
        return new StatementOfWorkMilestone(
            code,
            name,
            "",
            0,
            "",
            0.0,
            new Dictionary<string, string>(),
            0,
            StatementOfWorkMilestoneStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkMilestoneStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkMilestoneStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkMilestoneStatus.New:
                switch (next)
                {
                    case StatementOfWorkMilestoneStatus.Running:
                    case StatementOfWorkMilestoneStatus.Done:
                        return true;
                }
                return false;
            case StatementOfWorkMilestoneStatus.Queued:
                return next == StatementOfWorkMilestoneStatus.Running;
            case StatementOfWorkMilestoneStatus.Running:
                switch (next)
                {
                    case StatementOfWorkMilestoneStatus.Done:
                    case StatementOfWorkMilestoneStatus.Failed:
                        return true;
                }
                return false;
            case StatementOfWorkMilestoneStatus.Done:
                switch (next)
                {
                    case StatementOfWorkMilestoneStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"statement of work milestone"} {Code}";
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

    public double AdjustedTotal()
    {
        return Math.Round(Minimum - (double)GraceDays, 2);
    }

    public StatementOfWorkMilestone Copy()
    {
        return new StatementOfWorkMilestone(
            Code,
            Name,
            EmployeeId,
            WindowDays,
            OwnerId,
            Minimum,
            Attributes,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
