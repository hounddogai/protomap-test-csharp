namespace Acme.Workforce.Orgdesign.CostModel.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered cost model assignment is in its life cycle.</summary>
public enum TieredCostModelAssignmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One tiered cost model assignment, with what the planners need to act on it.</summary>
public sealed class TieredCostModelAssignment(string code, string name, int endsAt, string grade, Dictionary<string, string> attributes, double threshold, string checksum, bool active, string badge, TieredCostModelAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int EndsAt { get; set; } = endsAt;
    public string Grade { get; set; } = grade;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Threshold { get; set; } = threshold;
    public string Checksum { get; set; } = checksum;
    public bool Active { get; set; } = active;
    public string Badge { get; set; } = badge;
    public TieredCostModelAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a tiered cost model assignment in status created.</summary>
    public static TieredCostModelAssignment NewTieredCostModelAssignment(string code, string name)
    {
        return new TieredCostModelAssignment(
            code,
            name,
            0,
            "",
            new Dictionary<string, string>(),
            0.0,
            "",
            false,
            "",
            TieredCostModelAssignmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredCostModelAssignmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredCostModelAssignmentStatus next)
    {
        switch (Status)
        {
            case TieredCostModelAssignmentStatus.Created:
                switch (next)
                {
                    case TieredCostModelAssignmentStatus.Validated:
                    case TieredCostModelAssignmentStatus.Published:
                        return true;
                }
                return false;
            case TieredCostModelAssignmentStatus.Validated:
                return next == TieredCostModelAssignmentStatus.Published || next == TieredCostModelAssignmentStatus.Retired;
            case TieredCostModelAssignmentStatus.Published:
                return next == TieredCostModelAssignmentStatus.Retired;
        }
        return false;
    }

    public void MoveTo(TieredCostModelAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered cost model assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"tiered cost model assignment"} {Code}";
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

    public double ProjectedValue()
    {
        return Math.Round(Threshold + (double)EndsAt, 2);
    }

    public TieredCostModelAssignment Copy()
    {
        return new TieredCostModelAssignment(
            Code,
            Name,
            EndsAt,
            Grade,
            Attributes,
            Threshold,
            Checksum,
            Active,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
