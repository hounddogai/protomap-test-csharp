namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor manifest is in its life cycle.</summary>
public enum VisitorManifestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A visitor manifest as the visitor team keeps it.</summary>
public sealed class VisitorManifest(string code, string name, string locale, string reviewer, bool approved, string timezone, double hours, string managerId, double budget, string employeeId, int tenureMonths, VisitorManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public string Reviewer { get; set; } = reviewer;
    public bool Approved { get; set; } = approved;
    public string Timezone { get; set; } = timezone;
    public double Hours { get; set; } = hours;
    public string ManagerId { get; set; } = managerId;
    public double Budget { get; set; } = budget;
    public string EmployeeId { get; set; } = employeeId;
    public int TenureMonths { get; set; } = tenureMonths;
    public VisitorManifestStatus Status { get; set; } = status;

    /// <summary>Starts a visitor manifest in status new.</summary>
    public static VisitorManifest NewVisitorManifest(string code, string name)
    {
        return new VisitorManifest(code, name, "", "", false, "", 0.0, "", 0.0, "", 0, VisitorManifestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorManifestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorManifestStatus next)
    {
        switch (Status)
        {
            case VisitorManifestStatus.New:
                switch (next)
                {
                    case VisitorManifestStatus.Queued:
                        return true;
                }
                return false;
            case VisitorManifestStatus.Queued:
                switch (next)
                {
                    case VisitorManifestStatus.Running:
                        return true;
                }
                return false;
            case VisitorManifestStatus.Running:
                return next == VisitorManifestStatus.Done || next == VisitorManifestStatus.Failed;
            case VisitorManifestStatus.Done:
                switch (next)
                {
                    case VisitorManifestStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)TenureMonths + Budget, 2);
    }

    public VisitorManifest Copy()
    {
        return new VisitorManifest(
            Code,
            Name,
            Locale,
            Reviewer,
            Approved,
            Timezone,
            Hours,
            ManagerId,
            Budget,
            EmployeeId,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
