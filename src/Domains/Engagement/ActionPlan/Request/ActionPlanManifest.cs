namespace Acme.Workforce.Engagement.ActionPlan.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan manifest is in its life cycle.</summary>
public enum ActionPlanManifestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a action plan manifest and its current status.</summary>
public sealed class ActionPlanManifest(string code, string title, double minimum, int updatedAt, int revision, double ceiling, double floorValue, string location, int version, string jobCode, ActionPlanManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Revision { get; set; } = revision;
    public double Ceiling { get; set; } = ceiling;
    public double FloorValue { get; set; } = floorValue;
    public string Location { get; set; } = location;
    public int Version { get; set; } = version;
    public string JobCode { get; set; } = jobCode;
    public ActionPlanManifestStatus Status { get; set; } = status;

    /// <summary>Starts a action plan manifest in status new.</summary>
    public static ActionPlanManifest NewActionPlanManifest(string code, string title)
    {
        return new ActionPlanManifest(code, title, 0.0, 0, 0, 0.0, 0.0, "", 0, "", ActionPlanManifestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanManifestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanManifestStatus next)
    {
        switch (Status)
        {
            case ActionPlanManifestStatus.New:
                return next == ActionPlanManifestStatus.Failed;
            case ActionPlanManifestStatus.Queued:
                return next == ActionPlanManifestStatus.Running;
            case ActionPlanManifestStatus.Running:
                switch (next)
                {
                    case ActionPlanManifestStatus.Done:
                    case ActionPlanManifestStatus.Failed:
                        return true;
                }
                return false;
            case ActionPlanManifestStatus.Done:
                return next == ActionPlanManifestStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ActionPlanManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"action plan manifest"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Version + (double)UpdatedAt, 2);
    }

    public ActionPlanManifest Copy()
    {
        return new ActionPlanManifest(
            Code,
            Title,
            Minimum,
            UpdatedAt,
            Revision,
            Ceiling,
            FloorValue,
            Location,
            Version,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
