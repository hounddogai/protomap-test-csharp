namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive statement of work assignment is in its life cycle.</summary>
public enum AdaptiveStatementOfWorkAssignmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A adaptive statement of work assignment. Its code is unique within a tenant.</summary>
public sealed class AdaptiveStatementOfWorkAssignment(string code, string title, double ratio, string skill, string source, bool approved, AdaptiveStatementOfWorkAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public string Skill { get; set; } = skill;
    public string Source { get; set; } = source;
    public bool Approved { get; set; } = approved;
    public AdaptiveStatementOfWorkAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive statement of work assignment in status idle.</summary>
    public static AdaptiveStatementOfWorkAssignment NewAdaptiveStatementOfWorkAssignment(string code, string title)
    {
        return new AdaptiveStatementOfWorkAssignment(
            code,
            title,
            0.0,
            "",
            "",
            false,
            AdaptiveStatementOfWorkAssignmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveStatementOfWorkAssignmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveStatementOfWorkAssignmentStatus next)
    {
        switch (Status)
        {
            case AdaptiveStatementOfWorkAssignmentStatus.Idle:
                switch (next)
                {
                    case AdaptiveStatementOfWorkAssignmentStatus.Ready:
                        return true;
                }
                return false;
            case AdaptiveStatementOfWorkAssignmentStatus.Warming:
                switch (next)
                {
                    case AdaptiveStatementOfWorkAssignmentStatus.Draining:
                        return true;
                }
                return false;
            case AdaptiveStatementOfWorkAssignmentStatus.Ready:
                switch (next)
                {
                    case AdaptiveStatementOfWorkAssignmentStatus.Draining:
                    case AdaptiveStatementOfWorkAssignmentStatus.Stopped:
                        return true;
                }
                return false;
            case AdaptiveStatementOfWorkAssignmentStatus.Draining:
                switch (next)
                {
                    case AdaptiveStatementOfWorkAssignmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveStatementOfWorkAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive statement of work assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Ratio;
    }

    public AdaptiveStatementOfWorkAssignment Copy()
    {
        return new AdaptiveStatementOfWorkAssignment(Code, Title, Ratio, Skill, Source, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
