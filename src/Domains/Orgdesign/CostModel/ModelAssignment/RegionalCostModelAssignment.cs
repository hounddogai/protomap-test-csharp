namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional cost model assignment is in its life cycle.</summary>
public enum RegionalCostModelAssignmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a regional cost model assignment and its current status.</summary>
public sealed class RegionalCostModelAssignment(string code, string name, string skill, double rating, string externalRef, double cost, RegionalCostModelAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Skill { get; set; } = skill;
    public double Rating { get; set; } = rating;
    public string ExternalRef { get; set; } = externalRef;
    public double Cost { get; set; } = cost;
    public RegionalCostModelAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a regional cost model assignment in status draft.</summary>
    public static RegionalCostModelAssignment NewRegionalCostModelAssignment(string code, string name)
    {
        return new RegionalCostModelAssignment(
            code,
            name,
            "",
            0.0,
            "",
            0.0,
            RegionalCostModelAssignmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalCostModelAssignmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalCostModelAssignmentStatus next)
    {
        switch (Status)
        {
            case RegionalCostModelAssignmentStatus.Draft:
                switch (next)
                {
                    case RegionalCostModelAssignmentStatus.Closed:
                        return true;
                }
                return false;
            case RegionalCostModelAssignmentStatus.Pending:
                switch (next)
                {
                    case RegionalCostModelAssignmentStatus.Closed:
                        return true;
                }
                return false;
            case RegionalCostModelAssignmentStatus.Active:
                switch (next)
                {
                    case RegionalCostModelAssignmentStatus.Closed:
                        return true;
                }
                return false;
            case RegionalCostModelAssignmentStatus.Suspended:
                switch (next)
                {
                    case RegionalCostModelAssignmentStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalCostModelAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional cost model assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"regional cost model assignment"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round(Rating - Cost, 2);
    }

    public RegionalCostModelAssignment Copy()
    {
        return new RegionalCostModelAssignment(Code, Name, Skill, Rating, ExternalRef, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
