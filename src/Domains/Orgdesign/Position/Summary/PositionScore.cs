namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position score is in its life cycle.</summary>
public enum PositionScoreStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One position score, with what the planners need to act on it.</summary>
public sealed class PositionScore(string code, string name, int retries, string title, int quantity, string description, string employeeId, PositionScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public string Description { get; set; } = description;
    public string EmployeeId { get; set; } = employeeId;
    public PositionScoreStatus Status { get; set; } = status;

    /// <summary>Starts a position score in status planned.</summary>
    public static PositionScore NewPositionScore(string code, string name)
    {
        return new PositionScore(code, name, 0, "", 0, "", "", PositionScoreStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionScoreStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionScoreStatus next)
    {
        switch (Status)
        {
            case PositionScoreStatus.Planned:
                switch (next)
                {
                    case PositionScoreStatus.Scheduled:
                    case PositionScoreStatus.Cancelled:
                        return true;
                }
                return false;
            case PositionScoreStatus.Scheduled:
                switch (next)
                {
                    case PositionScoreStatus.Cancelled:
                        return true;
                }
                return false;
            case PositionScoreStatus.InProgress:
                switch (next)
                {
                    case PositionScoreStatus.Cancelled:
                        return true;
                }
                return false;
            case PositionScoreStatus.Completed:
                return next == PositionScoreStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PositionScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Retries * (double)Quantity, 2);
    }

    public PositionScore Copy()
    {
        return new PositionScore(Code, Name, Retries, Title, Quantity, Description, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
