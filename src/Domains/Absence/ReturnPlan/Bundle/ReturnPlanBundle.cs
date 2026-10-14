namespace Acme.Workforce.Absence.ReturnPlan.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan bundle is in its life cycle.</summary>
public enum ReturnPlanBundleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a return plan bundle and its current status.</summary>
public sealed class ReturnPlanBundle(string code, string name, double minimum, bool archived, string source, string contactEmail, string badge, double fte, double budget, ReturnPlanBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public bool Archived { get; set; } = archived;
    public string Source { get; set; } = source;
    public string ContactEmail { get; set; } = contactEmail;
    public string Badge { get; set; } = badge;
    public double Fte { get; set; } = fte;
    public double Budget { get; set; } = budget;
    public ReturnPlanBundleStatus Status { get; set; } = status;

    /// <summary>Starts a return plan bundle in status requested.</summary>
    public static ReturnPlanBundle NewReturnPlanBundle(string code, string name)
    {
        return new ReturnPlanBundle(code, name, 0.0, false, "", "", "", 0.0, 0.0, ReturnPlanBundleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanBundleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanBundleStatus next)
    {
        switch (Status)
        {
            case ReturnPlanBundleStatus.Requested:
                return next == ReturnPlanBundleStatus.Revoked || next == ReturnPlanBundleStatus.Expired;
            case ReturnPlanBundleStatus.Granted:
                switch (next)
                {
                    case ReturnPlanBundleStatus.Revoked:
                        return true;
                }
                return false;
            case ReturnPlanBundleStatus.Revoked:
                switch (next)
                {
                    case ReturnPlanBundleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReturnPlanBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Fte + Minimum, 2);
    }

    public ReturnPlanBundle Copy()
    {
        return new ReturnPlanBundle(Code, Name, Minimum, Archived, Source, ContactEmail, Badge, Fte, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
