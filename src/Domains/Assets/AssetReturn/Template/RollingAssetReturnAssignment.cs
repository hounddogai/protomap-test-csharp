namespace Acme.Workforce.Assets.AssetReturn.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling asset return assignment is in its life cycle.</summary>
public enum RollingAssetReturnAssignmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A rolling asset return assignment as the asset return team keeps it.</summary>
public sealed class RollingAssetReturnAssignment(string code, string title, List<string> tags, string salaryBand, string badge, double cost, bool visible, RollingAssetReturnAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public string SalaryBand { get; set; } = salaryBand;
    public string Badge { get; set; } = badge;
    public double Cost { get; set; } = cost;
    public bool Visible { get; set; } = visible;
    public RollingAssetReturnAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a rolling asset return assignment in status idle.</summary>
    public static RollingAssetReturnAssignment NewRollingAssetReturnAssignment(string code, string title)
    {
        return new RollingAssetReturnAssignment(
            code,
            title,
            new List<string>(),
            "",
            "",
            0.0,
            false,
            RollingAssetReturnAssignmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingAssetReturnAssignmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingAssetReturnAssignmentStatus next)
    {
        switch (Status)
        {
            case RollingAssetReturnAssignmentStatus.Idle:
                return next == RollingAssetReturnAssignmentStatus.Warming || next == RollingAssetReturnAssignmentStatus.Draining;
            case RollingAssetReturnAssignmentStatus.Warming:
                return next == RollingAssetReturnAssignmentStatus.Draining || next == RollingAssetReturnAssignmentStatus.Stopped;
            case RollingAssetReturnAssignmentStatus.Ready:
                return next == RollingAssetReturnAssignmentStatus.Draining;
            case RollingAssetReturnAssignmentStatus.Draining:
                switch (next)
                {
                    case RollingAssetReturnAssignmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingAssetReturnAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling asset return assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return Cost;
    }

    public RollingAssetReturnAssignment Copy()
    {
        return new RollingAssetReturnAssignment(Code, Title, Tags, SalaryBand, Badge, Cost, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
