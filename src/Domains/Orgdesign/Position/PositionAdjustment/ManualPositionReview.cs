namespace Acme.Workforce.Orgdesign.Position.PositionAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual position review is in its life cycle.</summary>
public enum ManualPositionReviewStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A manual position review. Its code is unique within a tenant.</summary>
public sealed class ManualPositionReview(string code, string name, string grade, double ceiling, string timezone, string unit, string region, ManualPositionReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public double Ceiling { get; set; } = ceiling;
    public string Timezone { get; set; } = timezone;
    public string Unit { get; set; } = unit;
    public string Region { get; set; } = region;
    public ManualPositionReviewStatus Status { get; set; } = status;

    /// <summary>Starts a manual position review in status open.</summary>
    public static ManualPositionReview NewManualPositionReview(string code, string name)
    {
        return new ManualPositionReview(code, name, "", 0.0, "", "", "", ManualPositionReviewStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualPositionReviewStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualPositionReviewStatus next)
    {
        switch (Status)
        {
            case ManualPositionReviewStatus.Open:
                return next == ManualPositionReviewStatus.Resolved || next == ManualPositionReviewStatus.Reopened;
            case ManualPositionReviewStatus.InReview:
                return next == ManualPositionReviewStatus.Resolved;
            case ManualPositionReviewStatus.Resolved:
                switch (next)
                {
                    case ManualPositionReviewStatus.Archived:
                        return true;
                }
                return false;
            case ManualPositionReviewStatus.Reopened:
                return next == ManualPositionReviewStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ManualPositionReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual position review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Ceiling;
    }

    public ManualPositionReview Copy()
    {
        return new ManualPositionReview(Code, Name, Grade, Ceiling, Timezone, Unit, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
