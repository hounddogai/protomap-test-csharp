namespace Acme.Workforce.Assets.Laptop.LaptopManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient laptop manifest is in its life cycle.</summary>
public enum LenientLaptopManifestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A lenient laptop manifest. Its code is unique within a tenant.</summary>
public sealed class LenientLaptopManifest(string code, string name, double score, string managerId, string phone, double rate, int attempts, int windowDays, double threshold, string team, LenientLaptopManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public string ManagerId { get; set; } = managerId;
    public string Phone { get; set; } = phone;
    public double Rate { get; set; } = rate;
    public int Attempts { get; set; } = attempts;
    public int WindowDays { get; set; } = windowDays;
    public double Threshold { get; set; } = threshold;
    public string Team { get; set; } = team;
    public LenientLaptopManifestStatus Status { get; set; } = status;

    /// <summary>Starts a lenient laptop manifest in status planned.</summary>
    public static LenientLaptopManifest NewLenientLaptopManifest(string code, string name)
    {
        return new LenientLaptopManifest(
            code,
            name,
            0.0,
            "",
            "",
            0.0,
            0,
            0,
            0.0,
            "",
            LenientLaptopManifestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientLaptopManifestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientLaptopManifestStatus next)
    {
        switch (Status)
        {
            case LenientLaptopManifestStatus.Planned:
                switch (next)
                {
                    case LenientLaptopManifestStatus.Scheduled:
                    case LenientLaptopManifestStatus.Cancelled:
                        return true;
                }
                return false;
            case LenientLaptopManifestStatus.Scheduled:
                switch (next)
                {
                    case LenientLaptopManifestStatus.InProgress:
                        return true;
                }
                return false;
            case LenientLaptopManifestStatus.InProgress:
                switch (next)
                {
                    case LenientLaptopManifestStatus.Completed:
                        return true;
                }
                return false;
            case LenientLaptopManifestStatus.Completed:
                switch (next)
                {
                    case LenientLaptopManifestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientLaptopManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient laptop manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient laptop manifest"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Rate - (double)WindowDays, 2);
    }

    public LenientLaptopManifest Copy()
    {
        return new LenientLaptopManifest(
            Code,
            Name,
            Score,
            ManagerId,
            Phone,
            Rate,
            Attempts,
            WindowDays,
            Threshold,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
