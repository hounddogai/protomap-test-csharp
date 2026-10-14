namespace Acme.Workforce.Assets.Laptop.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop manifest is in its life cycle.</summary>
public enum LaptopManifestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a laptop manifest and its current status.</summary>
public sealed class LaptopManifest(string code, string title, string externalRef, double budget, int leaveDays, string team, LaptopManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public double Budget { get; set; } = budget;
    public int LeaveDays { get; set; } = leaveDays;
    public string Team { get; set; } = team;
    public LaptopManifestStatus Status { get; set; } = status;

    /// <summary>Starts a laptop manifest in status planned.</summary>
    public static LaptopManifest NewLaptopManifest(string code, string title)
    {
        return new LaptopManifest(code, title, "", 0.0, 0, "", LaptopManifestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopManifestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopManifestStatus next)
    {
        switch (Status)
        {
            case LaptopManifestStatus.Planned:
                switch (next)
                {
                    case LaptopManifestStatus.InProgress:
                    case LaptopManifestStatus.Completed:
                        return true;
                }
                return false;
            case LaptopManifestStatus.Scheduled:
                switch (next)
                {
                    case LaptopManifestStatus.Completed:
                        return true;
                }
                return false;
            case LaptopManifestStatus.InProgress:
                switch (next)
                {
                    case LaptopManifestStatus.Cancelled:
                        return true;
                }
                return false;
            case LaptopManifestStatus.Completed:
                return next == LaptopManifestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LaptopManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"laptop manifest"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)LeaveDays * Budget, 2);
    }

    public LaptopManifest Copy()
    {
        return new LaptopManifest(Code, Title, ExternalRef, Budget, LeaveDays, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
