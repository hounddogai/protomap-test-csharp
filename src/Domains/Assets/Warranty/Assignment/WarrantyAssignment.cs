namespace Acme.Workforce.Assets.Warranty.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty assignment is in its life cycle.</summary>
public enum WarrantyAssignmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a warranty assignment and its current status.</summary>
public sealed class WarrantyAssignment(string code, string name, int leaveDays, int startsAt, double ratio, string department, WarrantyAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public int StartsAt { get; set; } = startsAt;
    public double Ratio { get; set; } = ratio;
    public string Department { get; set; } = department;
    public WarrantyAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a warranty assignment in status open.</summary>
    public static WarrantyAssignment NewWarrantyAssignment(string code, string name)
    {
        return new WarrantyAssignment(code, name, 0, 0, 0.0, "", WarrantyAssignmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyAssignmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyAssignmentStatus next)
    {
        switch (Status)
        {
            case WarrantyAssignmentStatus.Open:
                switch (next)
                {
                    case WarrantyAssignmentStatus.InReview:
                        return true;
                }
                return false;
            case WarrantyAssignmentStatus.InReview:
                switch (next)
                {
                    case WarrantyAssignmentStatus.Reopened:
                    case WarrantyAssignmentStatus.Archived:
                        return true;
                }
                return false;
            case WarrantyAssignmentStatus.Resolved:
                switch (next)
                {
                    case WarrantyAssignmentStatus.Archived:
                        return true;
                }
                return false;
            case WarrantyAssignmentStatus.Reopened:
                switch (next)
                {
                    case WarrantyAssignmentStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WarrantyAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"warranty assignment"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)LeaveDays * (double)StartsAt, 2);
    }

    public WarrantyAssignment Copy()
    {
        return new WarrantyAssignment(Code, Name, LeaveDays, StartsAt, Ratio, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
