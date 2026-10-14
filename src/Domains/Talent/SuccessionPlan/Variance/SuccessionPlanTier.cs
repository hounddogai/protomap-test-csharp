namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan tier is in its life cycle.</summary>
public enum SuccessionPlanTierStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a succession plan tier and its current status.</summary>
public sealed class SuccessionPlanTier(string code, string title, bool pinned, int limit, string location, SuccessionPlanTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public int Limit { get; set; } = limit;
    public string Location { get; set; } = location;
    public SuccessionPlanTierStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan tier in status created.</summary>
    public static SuccessionPlanTier NewSuccessionPlanTier(string code, string title)
    {
        return new SuccessionPlanTier(code, title, false, 0, "", SuccessionPlanTierStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanTierStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanTierStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanTierStatus.Created:
                return next == SuccessionPlanTierStatus.Validated || next == SuccessionPlanTierStatus.Retired;
            case SuccessionPlanTierStatus.Validated:
                return next == SuccessionPlanTierStatus.Published || next == SuccessionPlanTierStatus.Retired;
            case SuccessionPlanTierStatus.Published:
                switch (next)
                {
                    case SuccessionPlanTierStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"succession plan tier"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Limit;
    }

    public SuccessionPlanTier Copy()
    {
        return new SuccessionPlanTier(Code, Title, Pinned, Limit, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
