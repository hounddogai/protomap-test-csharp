namespace Acme.Workforce.Assets.AssetReturn.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return assignment is in its life cycle.</summary>
public enum AssetReturnAssignmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a asset return assignment and its current status.</summary>
public sealed class AssetReturnAssignment(string code, string title, string location, string costCenter, int tenureMonths, double maximum, string skill, string grade, bool pinned, AssetReturnAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public string CostCenter { get; set; } = costCenter;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Maximum { get; set; } = maximum;
    public string Skill { get; set; } = skill;
    public string Grade { get; set; } = grade;
    public bool Pinned { get; set; } = pinned;
    public AssetReturnAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a asset return assignment in status created.</summary>
    public static AssetReturnAssignment NewAssetReturnAssignment(string code, string title)
    {
        return new AssetReturnAssignment(
            code,
            title,
            "",
            "",
            0,
            0.0,
            "",
            "",
            false,
            AssetReturnAssignmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnAssignmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnAssignmentStatus next)
    {
        switch (Status)
        {
            case AssetReturnAssignmentStatus.Created:
                switch (next)
                {
                    case AssetReturnAssignmentStatus.Validated:
                        return true;
                }
                return false;
            case AssetReturnAssignmentStatus.Validated:
                switch (next)
                {
                    case AssetReturnAssignmentStatus.Published:
                        return true;
                }
                return false;
            case AssetReturnAssignmentStatus.Published:
                switch (next)
                {
                    case AssetReturnAssignmentStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedValue()
    {
        return Math.Round((double)TenureMonths * Maximum, 2);
    }

    public AssetReturnAssignment Copy()
    {
        return new AssetReturnAssignment(
            Code,
            Title,
            Location,
            CostCenter,
            TenureMonths,
            Maximum,
            Skill,
            Grade,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
