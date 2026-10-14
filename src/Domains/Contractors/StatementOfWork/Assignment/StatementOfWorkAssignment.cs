namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work assignment is in its life cycle.</summary>
public enum StatementOfWorkAssignmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One statement of work assignment, with what the planners need to act on it.</summary>
public sealed class StatementOfWorkAssignment(string code, string name, string costCenter, int sequence, Dictionary<string, string> attributes, double floorValue, double maximum, StatementOfWorkAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string CostCenter { get; set; } = costCenter;
    public int Sequence { get; set; } = sequence;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double FloorValue { get; set; } = floorValue;
    public double Maximum { get; set; } = maximum;
    public StatementOfWorkAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work assignment in status created.</summary>
    public static StatementOfWorkAssignment NewStatementOfWorkAssignment(string code, string name)
    {
        return new StatementOfWorkAssignment(
            code,
            name,
            "",
            0,
            new Dictionary<string, string>(),
            0.0,
            0.0,
            StatementOfWorkAssignmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkAssignmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkAssignmentStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkAssignmentStatus.Created:
                return next == StatementOfWorkAssignmentStatus.Validated;
            case StatementOfWorkAssignmentStatus.Validated:
                return next == StatementOfWorkAssignmentStatus.Retired;
            case StatementOfWorkAssignmentStatus.Published:
                switch (next)
                {
                    case StatementOfWorkAssignmentStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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

    public double NetVolume()
    {
        return Math.Round(Maximum * FloorValue, 2);
    }

    public StatementOfWorkAssignment Copy()
    {
        return new StatementOfWorkAssignment(
            Code,
            Name,
            CostCenter,
            Sequence,
            Attributes,
            FloorValue,
            Maximum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
