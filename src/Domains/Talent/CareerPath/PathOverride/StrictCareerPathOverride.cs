namespace Acme.Workforce.Talent.CareerPath.PathOverride;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict career path override is in its life cycle.</summary>
public enum StrictCareerPathOverrideStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a strict career path override and its current status.</summary>
public sealed class StrictCareerPathOverride(string code, string title, double discount, string notes, string unit, string badge, StrictCareerPathOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public string Notes { get; set; } = notes;
    public string Unit { get; set; } = unit;
    public string Badge { get; set; } = badge;
    public StrictCareerPathOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a strict career path override in status proposed.</summary>
    public static StrictCareerPathOverride NewStrictCareerPathOverride(string code, string title)
    {
        return new StrictCareerPathOverride(code, title, 0.0, "", "", "", StrictCareerPathOverrideStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictCareerPathOverrideStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictCareerPathOverrideStatus next)
    {
        switch (Status)
        {
            case StrictCareerPathOverrideStatus.Proposed:
                switch (next)
                {
                    case StrictCareerPathOverrideStatus.Approved:
                        return true;
                }
                return false;
            case StrictCareerPathOverrideStatus.Approved:
                return next == StrictCareerPathOverrideStatus.Rejected || next == StrictCareerPathOverrideStatus.Withdrawn;
            case StrictCareerPathOverrideStatus.Rejected:
                return next == StrictCareerPathOverrideStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(StrictCareerPathOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict career path override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Discount;
    }

    public StrictCareerPathOverride Copy()
    {
        return new StrictCareerPathOverride(Code, Title, Discount, Notes, Unit, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
