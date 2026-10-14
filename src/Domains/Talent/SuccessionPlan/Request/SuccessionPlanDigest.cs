namespace Acme.Workforce.Talent.SuccessionPlan.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan digest is in its life cycle.</summary>
public enum SuccessionPlanDigestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One succession plan digest, with what the planners need to act on it.</summary>
public sealed class SuccessionPlanDigest(string code, string label, int attempts, int graceDays, double ratio, double fte, string description, bool visible, string department, double cost, SuccessionPlanDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Attempts { get; set; } = attempts;
    public int GraceDays { get; set; } = graceDays;
    public double Ratio { get; set; } = ratio;
    public double Fte { get; set; } = fte;
    public string Description { get; set; } = description;
    public bool Visible { get; set; } = visible;
    public string Department { get; set; } = department;
    public double Cost { get; set; } = cost;
    public SuccessionPlanDigestStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan digest in status open.</summary>
    public static SuccessionPlanDigest NewSuccessionPlanDigest(string code, string label)
    {
        return new SuccessionPlanDigest(
            code,
            label,
            0,
            0,
            0.0,
            0.0,
            "",
            false,
            "",
            0.0,
            SuccessionPlanDigestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanDigestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanDigestStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanDigestStatus.Open:
                switch (next)
                {
                    case SuccessionPlanDigestStatus.InReview:
                    case SuccessionPlanDigestStatus.Resolved:
                        return true;
                }
                return false;
            case SuccessionPlanDigestStatus.InReview:
                switch (next)
                {
                    case SuccessionPlanDigestStatus.Resolved:
                        return true;
                }
                return false;
            case SuccessionPlanDigestStatus.Resolved:
                return next == SuccessionPlanDigestStatus.Reopened || next == SuccessionPlanDigestStatus.Archived;
            case SuccessionPlanDigestStatus.Reopened:
                switch (next)
                {
                    case SuccessionPlanDigestStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedLoad()
    {
        return Math.Round(Fte + (double)Attempts, 2);
    }

    public SuccessionPlanDigest Copy()
    {
        return new SuccessionPlanDigest(
            Code,
            Label,
            Attempts,
            GraceDays,
            Ratio,
            Fte,
            Description,
            Visible,
            Department,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
