namespace Acme.Workforce.Assets.Assignment.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment manifest is in its life cycle.</summary>
public enum AssignmentManifestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a assignment manifest and its current status.</summary>
public sealed class AssignmentManifest(string code, string label, string salaryBand, double hours, string name, string department, double threshold, double fte, AssignmentManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public double Hours { get; set; } = hours;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public double Threshold { get; set; } = threshold;
    public double Fte { get; set; } = fte;
    public AssignmentManifestStatus Status { get; set; } = status;

    /// <summary>Starts a assignment manifest in status draft.</summary>
    public static AssignmentManifest NewAssignmentManifest(string code, string label)
    {
        return new AssignmentManifest(code, label, "", 0.0, "", "", 0.0, 0.0, AssignmentManifestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentManifestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentManifestStatus next)
    {
        switch (Status)
        {
            case AssignmentManifestStatus.Draft:
                switch (next)
                {
                    case AssignmentManifestStatus.Pending:
                    case AssignmentManifestStatus.Suspended:
                        return true;
                }
                return false;
            case AssignmentManifestStatus.Pending:
                return next == AssignmentManifestStatus.Active;
            case AssignmentManifestStatus.Active:
                switch (next)
                {
                    case AssignmentManifestStatus.Suspended:
                    case AssignmentManifestStatus.Closed:
                        return true;
                }
                return false;
            case AssignmentManifestStatus.Suspended:
                return next == AssignmentManifestStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AssignmentManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"assignment manifest"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(Hours + Fte, 2);
    }

    public AssignmentManifest Copy()
    {
        return new AssignmentManifest(Code, Label, SalaryBand, Hours, Name, Department, Threshold, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
