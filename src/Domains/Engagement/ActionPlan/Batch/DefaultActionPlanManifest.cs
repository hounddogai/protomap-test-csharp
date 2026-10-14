namespace Acme.Workforce.Engagement.ActionPlan.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default action plan manifest is in its life cycle.</summary>
public enum DefaultActionPlanManifestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A default action plan manifest. Its code is unique within a tenant.</summary>
public sealed class DefaultActionPlanManifest(string code, string name, double hours, int windowDays, string notes, DefaultActionPlanManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Hours { get; set; } = hours;
    public int WindowDays { get; set; } = windowDays;
    public string Notes { get; set; } = notes;
    public DefaultActionPlanManifestStatus Status { get; set; } = status;

    /// <summary>Starts a default action plan manifest in status draft.</summary>
    public static DefaultActionPlanManifest NewDefaultActionPlanManifest(string code, string name)
    {
        return new DefaultActionPlanManifest(code, name, 0.0, 0, "", DefaultActionPlanManifestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultActionPlanManifestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultActionPlanManifestStatus next)
    {
        switch (Status)
        {
            case DefaultActionPlanManifestStatus.Draft:
                return next == DefaultActionPlanManifestStatus.Pending || next == DefaultActionPlanManifestStatus.Active;
            case DefaultActionPlanManifestStatus.Pending:
                return next == DefaultActionPlanManifestStatus.Suspended || next == DefaultActionPlanManifestStatus.Closed;
            case DefaultActionPlanManifestStatus.Active:
                return next == DefaultActionPlanManifestStatus.Suspended || next == DefaultActionPlanManifestStatus.Closed;
            case DefaultActionPlanManifestStatus.Suspended:
                switch (next)
                {
                    case DefaultActionPlanManifestStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultActionPlanManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default action plan manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"default action plan manifest"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)WindowDays - Hours, 2);
    }

    public DefaultActionPlanManifest Copy()
    {
        return new DefaultActionPlanManifest(Code, Name, Hours, WindowDays, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
