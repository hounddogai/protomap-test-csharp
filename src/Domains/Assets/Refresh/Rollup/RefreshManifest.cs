namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh manifest is in its life cycle.</summary>
public enum RefreshManifestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a refresh manifest and its current status.</summary>
public sealed class RefreshManifest(string code, string label, string jobCode, bool active, int leaveDays, double hours, double ceiling, string skill, string reviewer, RefreshManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public bool Active { get; set; } = active;
    public int LeaveDays { get; set; } = leaveDays;
    public double Hours { get; set; } = hours;
    public double Ceiling { get; set; } = ceiling;
    public string Skill { get; set; } = skill;
    public string Reviewer { get; set; } = reviewer;
    public RefreshManifestStatus Status { get; set; } = status;

    /// <summary>Starts a refresh manifest in status created.</summary>
    public static RefreshManifest NewRefreshManifest(string code, string label)
    {
        return new RefreshManifest(code, label, "", false, 0, 0.0, 0.0, "", "", RefreshManifestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshManifestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshManifestStatus next)
    {
        switch (Status)
        {
            case RefreshManifestStatus.Created:
                switch (next)
                {
                    case RefreshManifestStatus.Published:
                        return true;
                }
                return false;
            case RefreshManifestStatus.Validated:
                return next == RefreshManifestStatus.Published || next == RefreshManifestStatus.Retired;
            case RefreshManifestStatus.Published:
                switch (next)
                {
                    case RefreshManifestStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Ceiling * (double)LeaveDays, 2);
    }

    public RefreshManifest Copy()
    {
        return new RefreshManifest(Code, Label, JobCode, Active, LeaveDays, Hours, Ceiling, Skill, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
