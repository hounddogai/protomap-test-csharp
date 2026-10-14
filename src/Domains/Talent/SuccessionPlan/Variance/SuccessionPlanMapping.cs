namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan mapping is in its life cycle.</summary>
public enum SuccessionPlanMappingStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One succession plan mapping, with what the planners need to act on it.</summary>
public sealed class SuccessionPlanMapping(string code, string title, int quantity, double fte, int retries, string grade, SuccessionPlanMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public double Fte { get; set; } = fte;
    public int Retries { get; set; } = retries;
    public string Grade { get; set; } = grade;
    public SuccessionPlanMappingStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan mapping in status requested.</summary>
    public static SuccessionPlanMapping NewSuccessionPlanMapping(string code, string title)
    {
        return new SuccessionPlanMapping(code, title, 0, 0.0, 0, "", SuccessionPlanMappingStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanMappingStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanMappingStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanMappingStatus.Requested:
                switch (next)
                {
                    case SuccessionPlanMappingStatus.Granted:
                    case SuccessionPlanMappingStatus.Revoked:
                        return true;
                }
                return false;
            case SuccessionPlanMappingStatus.Granted:
                return next == SuccessionPlanMappingStatus.Revoked || next == SuccessionPlanMappingStatus.Expired;
            case SuccessionPlanMappingStatus.Revoked:
                return next == SuccessionPlanMappingStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetExposure()
    {
        return Math.Round(Fte * (double)Quantity, 2);
    }

    public SuccessionPlanMapping Copy()
    {
        return new SuccessionPlanMapping(Code, Title, Quantity, Fte, Retries, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
