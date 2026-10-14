namespace Acme.Workforce.Absence.ReturnPlan.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan manifest is in its life cycle.</summary>
public enum ReturnPlanManifestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a return plan manifest and its current status.</summary>
public sealed class ReturnPlanManifest(string code, string label, string team, int windowDays, string name, string reviewer, string source, int startsAt, ReturnPlanManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public int WindowDays { get; set; } = windowDays;
    public string Name { get; set; } = name;
    public string Reviewer { get; set; } = reviewer;
    public string Source { get; set; } = source;
    public int StartsAt { get; set; } = startsAt;
    public ReturnPlanManifestStatus Status { get; set; } = status;

    /// <summary>Starts a return plan manifest in status open.</summary>
    public static ReturnPlanManifest NewReturnPlanManifest(string code, string label)
    {
        return new ReturnPlanManifest(code, label, "", 0, "", "", "", 0, ReturnPlanManifestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanManifestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanManifestStatus next)
    {
        switch (Status)
        {
            case ReturnPlanManifestStatus.Open:
                switch (next)
                {
                    case ReturnPlanManifestStatus.Archived:
                        return true;
                }
                return false;
            case ReturnPlanManifestStatus.InReview:
                return next == ReturnPlanManifestStatus.Reopened;
            case ReturnPlanManifestStatus.Resolved:
                return next == ReturnPlanManifestStatus.Reopened || next == ReturnPlanManifestStatus.Archived;
            case ReturnPlanManifestStatus.Reopened:
                switch (next)
                {
                    case ReturnPlanManifestStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReturnPlanManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)WindowDays + (double)StartsAt, 2);
    }

    public ReturnPlanManifest Copy()
    {
        return new ReturnPlanManifest(Code, Label, Team, WindowDays, Name, Reviewer, Source, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
