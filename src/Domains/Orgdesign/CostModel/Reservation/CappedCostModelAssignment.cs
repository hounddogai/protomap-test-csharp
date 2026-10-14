namespace Acme.Workforce.Orgdesign.CostModel.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped cost model assignment is in its life cycle.</summary>
public enum CappedCostModelAssignmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One capped cost model assignment, with what the planners need to act on it.</summary>
public sealed class CappedCostModelAssignment(string code, string name, string channel, double budget, double maximum, CappedCostModelAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public double Budget { get; set; } = budget;
    public double Maximum { get; set; } = maximum;
    public CappedCostModelAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a capped cost model assignment in status created.</summary>
    public static CappedCostModelAssignment NewCappedCostModelAssignment(string code, string name)
    {
        return new CappedCostModelAssignment(code, name, "", 0.0, 0.0, CappedCostModelAssignmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedCostModelAssignmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedCostModelAssignmentStatus next)
    {
        switch (Status)
        {
            case CappedCostModelAssignmentStatus.Created:
                switch (next)
                {
                    case CappedCostModelAssignmentStatus.Validated:
                        return true;
                }
                return false;
            case CappedCostModelAssignmentStatus.Validated:
                return next == CappedCostModelAssignmentStatus.Published;
            case CappedCostModelAssignmentStatus.Published:
                switch (next)
                {
                    case CappedCostModelAssignmentStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedCostModelAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped cost model assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Maximum * Budget, 2);
    }

    public CappedCostModelAssignment Copy()
    {
        return new CappedCostModelAssignment(Code, Name, Channel, Budget, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
