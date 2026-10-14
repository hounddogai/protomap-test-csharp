namespace Acme.Workforce.Talent.CareerPath.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path target is in its life cycle.</summary>
public enum CareerPathTargetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One career path target, with what the planners need to act on it.</summary>
public sealed class CareerPathTarget(string code, string title, double ratio, string externalRef, int tenureMonths, string summary, bool approved, double discount, double margin, int sequence, CareerPathTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public string ExternalRef { get; set; } = externalRef;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Summary { get; set; } = summary;
    public bool Approved { get; set; } = approved;
    public double Discount { get; set; } = discount;
    public double Margin { get; set; } = margin;
    public int Sequence { get; set; } = sequence;
    public CareerPathTargetStatus Status { get; set; } = status;

    /// <summary>Starts a career path target in status new.</summary>
    public static CareerPathTarget NewCareerPathTarget(string code, string title)
    {
        return new CareerPathTarget(code, title, 0.0, "", 0, "", false, 0.0, 0.0, 0, CareerPathTargetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathTargetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathTargetStatus next)
    {
        switch (Status)
        {
            case CareerPathTargetStatus.New:
                switch (next)
                {
                    case CareerPathTargetStatus.Queued:
                    case CareerPathTargetStatus.Done:
                        return true;
                }
                return false;
            case CareerPathTargetStatus.Queued:
                switch (next)
                {
                    case CareerPathTargetStatus.Failed:
                        return true;
                }
                return false;
            case CareerPathTargetStatus.Running:
                switch (next)
                {
                    case CareerPathTargetStatus.Failed:
                        return true;
                }
                return false;
            case CareerPathTargetStatus.Done:
                switch (next)
                {
                    case CareerPathTargetStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CareerPathTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Discount - Margin, 2);
    }

    public CareerPathTarget Copy()
    {
        return new CareerPathTarget(
            Code,
            Title,
            Ratio,
            ExternalRef,
            TenureMonths,
            Summary,
            Approved,
            Discount,
            Margin,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
