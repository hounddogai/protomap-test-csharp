namespace Acme.Workforce.Assets.Refresh.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling refresh manifest is in its life cycle.</summary>
public enum RollingRefreshManifestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One rolling refresh manifest, with what the planners need to act on it.</summary>
public sealed class RollingRefreshManifest(string code, string title, double margin, int leaveDays, string ownerId, string skill, string unit, string channel, string timezone, RollingRefreshManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public int LeaveDays { get; set; } = leaveDays;
    public string OwnerId { get; set; } = ownerId;
    public string Skill { get; set; } = skill;
    public string Unit { get; set; } = unit;
    public string Channel { get; set; } = channel;
    public string Timezone { get; set; } = timezone;
    public RollingRefreshManifestStatus Status { get; set; } = status;

    /// <summary>Starts a rolling refresh manifest in status planned.</summary>
    public static RollingRefreshManifest NewRollingRefreshManifest(string code, string title)
    {
        return new RollingRefreshManifest(
            code,
            title,
            0.0,
            0,
            "",
            "",
            "",
            "",
            "",
            RollingRefreshManifestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingRefreshManifestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingRefreshManifestStatus next)
    {
        switch (Status)
        {
            case RollingRefreshManifestStatus.Planned:
                switch (next)
                {
                    case RollingRefreshManifestStatus.Scheduled:
                    case RollingRefreshManifestStatus.Completed:
                        return true;
                }
                return false;
            case RollingRefreshManifestStatus.Scheduled:
                switch (next)
                {
                    case RollingRefreshManifestStatus.Completed:
                    case RollingRefreshManifestStatus.Cancelled:
                        return true;
                }
                return false;
            case RollingRefreshManifestStatus.InProgress:
                return next == RollingRefreshManifestStatus.Completed || next == RollingRefreshManifestStatus.Cancelled;
            case RollingRefreshManifestStatus.Completed:
                return next == RollingRefreshManifestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RollingRefreshManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling refresh manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rolling refresh manifest"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)LeaveDays + Margin, 2);
    }

    public RollingRefreshManifest Copy()
    {
        return new RollingRefreshManifest(
            Code,
            Title,
            Margin,
            LeaveDays,
            OwnerId,
            Skill,
            Unit,
            Channel,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
