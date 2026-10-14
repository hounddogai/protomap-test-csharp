namespace Acme.Workforce.Talent.CareerPath.GlobalPathRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive career path exception is in its life cycle.</summary>
public enum AdaptiveCareerPathExceptionStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A adaptive career path exception as the career path team keeps it.</summary>
public sealed class AdaptiveCareerPathException(string code, string label, int tenureMonths, string badge, double discount, string checksum, string category, string team, AdaptiveCareerPathExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Badge { get; set; } = badge;
    public double Discount { get; set; } = discount;
    public string Checksum { get; set; } = checksum;
    public string Category { get; set; } = category;
    public string Team { get; set; } = team;
    public AdaptiveCareerPathExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive career path exception in status idle.</summary>
    public static AdaptiveCareerPathException NewAdaptiveCareerPathException(string code, string label)
    {
        return new AdaptiveCareerPathException(
            code,
            label,
            0,
            "",
            0.0,
            "",
            "",
            "",
            AdaptiveCareerPathExceptionStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveCareerPathExceptionStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveCareerPathExceptionStatus next)
    {
        switch (Status)
        {
            case AdaptiveCareerPathExceptionStatus.Idle:
                return next == AdaptiveCareerPathExceptionStatus.Stopped;
            case AdaptiveCareerPathExceptionStatus.Warming:
                switch (next)
                {
                    case AdaptiveCareerPathExceptionStatus.Ready:
                    case AdaptiveCareerPathExceptionStatus.Stopped:
                        return true;
                }
                return false;
            case AdaptiveCareerPathExceptionStatus.Ready:
                switch (next)
                {
                    case AdaptiveCareerPathExceptionStatus.Draining:
                    case AdaptiveCareerPathExceptionStatus.Stopped:
                        return true;
                }
                return false;
            case AdaptiveCareerPathExceptionStatus.Draining:
                return next == AdaptiveCareerPathExceptionStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AdaptiveCareerPathExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive career path exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetExposure()
    {
        return Math.Round((double)TenureMonths - Discount, 2);
    }

    public AdaptiveCareerPathException Copy()
    {
        return new AdaptiveCareerPathException(
            Code,
            Label,
            TenureMonths,
            Badge,
            Discount,
            Checksum,
            Category,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
