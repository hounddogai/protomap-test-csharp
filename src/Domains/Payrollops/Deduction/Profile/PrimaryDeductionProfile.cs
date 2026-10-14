namespace Acme.Workforce.Payrollops.Deduction.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary deduction profile is in its life cycle.</summary>
public enum PrimaryDeductionProfileStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A primary deduction profile as the deduction team keeps it.</summary>
public sealed class PrimaryDeductionProfile(string code, string title, int tenureMonths, int capacity, int attempts, string location, double weight, string team, Dictionary<string, string> attributes, int retries, PrimaryDeductionProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Capacity { get; set; } = capacity;
    public int Attempts { get; set; } = attempts;
    public string Location { get; set; } = location;
    public double Weight { get; set; } = weight;
    public string Team { get; set; } = team;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Retries { get; set; } = retries;
    public PrimaryDeductionProfileStatus Status { get; set; } = status;

    /// <summary>Starts a primary deduction profile in status planned.</summary>
    public static PrimaryDeductionProfile NewPrimaryDeductionProfile(string code, string title)
    {
        return new PrimaryDeductionProfile(
            code,
            title,
            0,
            0,
            0,
            "",
            0.0,
            "",
            new Dictionary<string, string>(),
            0,
            PrimaryDeductionProfileStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryDeductionProfileStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryDeductionProfileStatus next)
    {
        switch (Status)
        {
            case PrimaryDeductionProfileStatus.Planned:
                switch (next)
                {
                    case PrimaryDeductionProfileStatus.Completed:
                    case PrimaryDeductionProfileStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimaryDeductionProfileStatus.Scheduled:
                return next == PrimaryDeductionProfileStatus.Cancelled;
            case PrimaryDeductionProfileStatus.InProgress:
                return next == PrimaryDeductionProfileStatus.Completed || next == PrimaryDeductionProfileStatus.Cancelled;
            case PrimaryDeductionProfileStatus.Completed:
                return next == PrimaryDeductionProfileStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PrimaryDeductionProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary deduction profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"primary deduction profile"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Attempts * (double)Retries, 2);
    }

    public PrimaryDeductionProfile Copy()
    {
        return new PrimaryDeductionProfile(
            Code,
            Title,
            TenureMonths,
            Capacity,
            Attempts,
            Location,
            Weight,
            Team,
            Attributes,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
