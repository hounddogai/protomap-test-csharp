namespace Acme.Workforce.Absence.ReturnPlan.PlanManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan tier is in its life cycle.</summary>
public enum ReturnPlanTierStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A return plan tier as the return plan team keeps it.</summary>
public sealed class ReturnPlanTier(string code, string label, int revision, int attempts, string checksum, double amount, ReturnPlanTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public int Attempts { get; set; } = attempts;
    public string Checksum { get; set; } = checksum;
    public double Amount { get; set; } = amount;
    public ReturnPlanTierStatus Status { get; set; } = status;

    /// <summary>Starts a return plan tier in status created.</summary>
    public static ReturnPlanTier NewReturnPlanTier(string code, string label)
    {
        return new ReturnPlanTier(code, label, 0, 0, "", 0.0, ReturnPlanTierStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanTierStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanTierStatus next)
    {
        switch (Status)
        {
            case ReturnPlanTierStatus.Created:
                switch (next)
                {
                    case ReturnPlanTierStatus.Published:
                    case ReturnPlanTierStatus.Retired:
                        return true;
                }
                return false;
            case ReturnPlanTierStatus.Validated:
                return next == ReturnPlanTierStatus.Published || next == ReturnPlanTierStatus.Retired;
            case ReturnPlanTierStatus.Published:
                return next == ReturnPlanTierStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ReturnPlanTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Amount * (double)Revision, 2);
    }

    public ReturnPlanTier Copy()
    {
        return new ReturnPlanTier(Code, Label, Revision, Attempts, Checksum, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
