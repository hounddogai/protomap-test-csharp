namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling succession plan override is in its life cycle.</summary>
public enum RollingSuccessionPlanOverrideStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a rolling succession plan override and its current status.</summary>
public sealed class RollingSuccessionPlanOverride(string code, string name, string contactEmail, int revision, double amount, int updatedAt, double maximum, string region, int graceDays, RollingSuccessionPlanOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public int Revision { get; set; } = revision;
    public double Amount { get; set; } = amount;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Maximum { get; set; } = maximum;
    public string Region { get; set; } = region;
    public int GraceDays { get; set; } = graceDays;
    public RollingSuccessionPlanOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a rolling succession plan override in status created.</summary>
    public static RollingSuccessionPlanOverride NewRollingSuccessionPlanOverride(string code, string name)
    {
        return new RollingSuccessionPlanOverride(
            code,
            name,
            "",
            0,
            0.0,
            0,
            0.0,
            "",
            0,
            RollingSuccessionPlanOverrideStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingSuccessionPlanOverrideStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingSuccessionPlanOverrideStatus next)
    {
        switch (Status)
        {
            case RollingSuccessionPlanOverrideStatus.Created:
                return next == RollingSuccessionPlanOverrideStatus.Published;
            case RollingSuccessionPlanOverrideStatus.Validated:
                switch (next)
                {
                    case RollingSuccessionPlanOverrideStatus.Published:
                    case RollingSuccessionPlanOverrideStatus.Retired:
                        return true;
                }
                return false;
            case RollingSuccessionPlanOverrideStatus.Published:
                switch (next)
                {
                    case RollingSuccessionPlanOverrideStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingSuccessionPlanOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling succession plan override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"rolling succession plan override"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)GraceDays * Amount, 2);
    }

    public RollingSuccessionPlanOverride Copy()
    {
        return new RollingSuccessionPlanOverride(
            Code,
            Name,
            ContactEmail,
            Revision,
            Amount,
            UpdatedAt,
            Maximum,
            Region,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
